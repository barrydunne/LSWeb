using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateChangeSet;
using Foundation.Application.Commands.CreateStack;
using Foundation.Application.Commands.DeleteChangeSet;
using Foundation.Application.Commands.DeleteStack;
using Foundation.Application.Commands.DetectStackDrift;
using Foundation.Application.Commands.ExecuteChangeSet;
using Foundation.Application.Commands.UpdateStack;
using Foundation.Application.Queries.DescribeChangeSet;
using Foundation.Application.Queries.GetDriftStatus;
using Foundation.Application.Queries.GetStack;
using Foundation.Application.Queries.GetStackTemplate;
using Foundation.Application.Queries.ListChangeSets;
using Foundation.Application.Queries.ListExports;
using Foundation.Application.Queries.ListImports;
using Foundation.Application.Queries.ListResourceDrifts;
using Foundation.Application.Queries.ListStackEvents;
using Foundation.Application.Queries.ListStackResources;
using Foundation.Application.Queries.ListStacks;
using Foundation.Application.Queries.ValidateTemplate;
using Foundation.Domain.CloudFormation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class CloudFormationControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<CloudFormationController> _logger =
        Substitute.For<ILogger<CloudFormationController>>();

    private CloudFormationController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListStacks_WhenQuerySucceeds_ReturnsOkWithStacks()
    {
        // Arrange
        IReadOnlyList<CloudFormationStackSummary> stacks =
        [
            new(
                "orders-stack",
                "arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc",
                "CREATE_COMPLETE",
                "Orders processing stack",
                DateTimeOffset.UnixEpoch,
                DateTimeOffset.UnixEpoch.AddHours(1)),
        ];
        _sender
            .Send(Arg.Any<ListStacksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListStacksQueryResult>>(
                new ListStacksQueryResult(stacks)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListStacks(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CloudFormationStackListResponse>>().Subject;
        var stack = ok.Value!.Stacks.Should().ContainSingle().Subject;
        stack.StackName.Should().Be("orders-stack");
        stack.StackId.Should()
            .Be("arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc");
        stack.StackStatus.Should().Be("CREATE_COMPLETE");
        stack.Description.Should().Be("Orders processing stack");
        stack.CreationTime.Should().Be(DateTimeOffset.UnixEpoch);
        stack.LastUpdatedTime.Should().Be(DateTimeOffset.UnixEpoch.AddHours(1));
    }

    [Fact]
    public async Task ListStacks_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListStacksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListStacksQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListStacks(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetStack_WhenQuerySucceeds_ReturnsOkWithDetailsAndForwardsName()
    {
        // Arrange
        const string name = "orders-stack";
        var detail = new CloudFormationStackDetail(
            name,
            "arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc",
            "UPDATE_COMPLETE",
            "User initiated",
            "Orders processing stack",
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch.AddHours(2),
            [new("Env", "dev")],
            [new("Url", "https://x", "The url", "orders-url")],
            [new("team", "orders")],
            ["CAPABILITY_IAM"]);
        GetStackQuery? captured = null;
        _sender
            .Send(Arg.Do<GetStackQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetStackQueryResult>>(
                new GetStackQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetStack(name, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CloudFormationStackDetailResponse>>().Subject;
        ok.Value!.StackName.Should().Be("orders-stack");
        ok.Value.StackId.Should().Be("arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc");
        ok.Value.StackStatus.Should().Be("UPDATE_COMPLETE");
        ok.Value.StackStatusReason.Should().Be("User initiated");
        ok.Value.Description.Should().Be("Orders processing stack");
        ok.Value.CreationTime.Should().Be(DateTimeOffset.UnixEpoch);
        ok.Value.LastUpdatedTime.Should().Be(DateTimeOffset.UnixEpoch.AddHours(2));
        var parameter = ok.Value.Parameters.Should().ContainSingle().Subject;
        parameter.ParameterKey.Should().Be("Env");
        parameter.ParameterValue.Should().Be("dev");
        var output = ok.Value.Outputs.Should().ContainSingle().Subject;
        output.OutputKey.Should().Be("Url");
        output.OutputValue.Should().Be("https://x");
        output.Description.Should().Be("The url");
        output.ExportName.Should().Be("orders-url");
        var tag = ok.Value.Tags.Should().ContainSingle().Subject;
        tag.Key.Should().Be("team");
        tag.Value.Should().Be("orders");
        ok.Value.Capabilities.Should().ContainSingle().Which.Should().Be("CAPABILITY_IAM");
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be(name);
    }

    [Fact]
    public async Task GetStack_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetStackQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetStackQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetStack("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetStackTemplate_WhenQuerySucceeds_ReturnsOkWithTemplateAndForwardsName()
    {
        // Arrange
        const string name = "orders-stack";
        var template = new CloudFormationStackTemplate("{\"Resources\":{}}", "json");
        GetStackTemplateQuery? captured = null;
        _sender
            .Send(
                Arg.Do<GetStackTemplateQuery>(query => captured = query),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetStackTemplateQueryResult>>(
                new GetStackTemplateQueryResult(template)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetStackTemplate(name, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CloudFormationStackTemplateResponse>>().Subject;
        ok.Value!.TemplateBody.Should().Be("{\"Resources\":{}}");
        ok.Value.Format.Should().Be("json");
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be(name);
    }

    [Fact]
    public async Task GetStackTemplate_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetStackTemplateQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetStackTemplateQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetStackTemplate("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListStackResources_WhenQuerySucceeds_ReturnsOkWithResourcesAndForwardsName()
    {
        // Arrange
        const string name = "orders-stack";
        var timestamp = DateTimeOffset.UnixEpoch.AddHours(3);
        IReadOnlyList<StackResource> resources =
        [
            new(
                "OrdersQueue",
                "orders-queue",
                "AWS::SQS::Queue",
                "CREATE_COMPLETE",
                "Resource creation initiated",
                timestamp.UtcDateTime),
        ];
        ListStackResourcesQuery? captured = null;
        _sender
            .Send(
                Arg.Do<ListStackResourcesQuery>(query => captured = query),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListStackResourcesQueryResult>>(
                new ListStackResourcesQueryResult(resources)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListStackResources(name, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CloudFormationStackResourceListResponse>>().Subject;
        var resource = ok.Value!.Resources.Should().ContainSingle().Subject;
        resource.LogicalResourceId.Should().Be("OrdersQueue");
        resource.PhysicalResourceId.Should().Be("orders-queue");
        resource.ResourceType.Should().Be("AWS::SQS::Queue");
        resource.ResourceStatus.Should().Be("CREATE_COMPLETE");
        resource.ResourceStatusReason.Should().Be("Resource creation initiated");
        resource.LastUpdatedTime.Should().Be(timestamp);
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be(name);
    }

    [Fact]
    public async Task ListStackResources_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListStackResourcesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListStackResourcesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListStackResources("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListStackEvents_WhenQuerySucceeds_ReturnsOkWithEventsAndForwardsName()
    {
        // Arrange
        const string name = "orders-stack";
        var timestamp = DateTimeOffset.UnixEpoch.AddHours(4);
        IReadOnlyList<StackEvent> events =
        [
            new(
                "event-1",
                timestamp.UtcDateTime,
                "OrdersQueue",
                "orders-queue",
                "AWS::SQS::Queue",
                "CREATE_COMPLETE",
                "Resource creation initiated"),
        ];
        ListStackEventsQuery? captured = null;
        _sender
            .Send(
                Arg.Do<ListStackEventsQuery>(query => captured = query),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListStackEventsQueryResult>>(
                new ListStackEventsQueryResult(events)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListStackEvents(name, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CloudFormationStackEventListResponse>>().Subject;
        var stackEvent = ok.Value!.Events.Should().ContainSingle().Subject;
        stackEvent.EventId.Should().Be("event-1");
        stackEvent.Timestamp.Should().Be(timestamp);
        stackEvent.LogicalResourceId.Should().Be("OrdersQueue");
        stackEvent.PhysicalResourceId.Should().Be("orders-queue");
        stackEvent.ResourceType.Should().Be("AWS::SQS::Queue");
        stackEvent.ResourceStatus.Should().Be("CREATE_COMPLETE");
        stackEvent.ResourceStatusReason.Should().Be("Resource creation initiated");
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be(name);
    }

    [Fact]
    public async Task ListStackEvents_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListStackEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListStackEventsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListStackEvents("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateStack_WhenCommandSucceeds_ReturnsCreatedWithStackIdAndForwardsRequest()
    {
        // Arrange
        var request = new CloudFormationStackCreateRequest(
            "orders-stack",
            "{\"Resources\":{}}",
            null,
            [new("Env", "dev")],
            ["CAPABILITY_IAM"]);
        CreateStackCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateStackCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(
                "arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc"));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateStack(request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created<CloudFormationStackOperationResponse>>().Subject;
        created.Value!.StackId.Should()
            .Be("arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc");
        created.Location.Should().Be("/api/services/cloudformation/stack?name=orders-stack");
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be("orders-stack");
        captured.TemplateBody.Should().Be("{\"Resources\":{}}");
        captured.TemplateUrl.Should().BeNull();
        var parameter = captured.Parameters.Should().ContainSingle().Subject;
        parameter.ParameterKey.Should().Be("Env");
        parameter.ParameterValue.Should().Be("dev");
        captured.Capabilities.Should().ContainSingle().Which.Should().Be("CAPABILITY_IAM");
    }

    [Fact]
    public async Task CreateStack_WhenRequestOmitsCollections_DefaultsToEmpty()
    {
        // Arrange
        var request = new CloudFormationStackCreateRequest(
            "orders-stack",
            "{\"Resources\":{}}",
            null,
            null,
            null);
        CreateStackCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateStackCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("arn:stack"));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateStack(request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Created<CloudFormationStackOperationResponse>>();
        captured.Should().NotBeNull();
        captured!.Parameters.Should().BeEmpty();
        captured.Capabilities.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateStack_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new CloudFormationStackCreateRequest("orders-stack", "{}", null, null, null);
        _sender
            .Send(Arg.Any<CreateStackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateStack(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ValidateTemplate_WhenQuerySucceeds_ReturnsOkWithValidationAndForwardsRequest()
    {
        // Arrange
        var request = new CloudFormationTemplateValidationRequest("{\"Resources\":{}}", null);
        var validation = new TemplateValidationResult(
            "An example template",
            "Requires IAM",
            ["CAPABILITY_IAM"],
            [new TemplateValidationParameter("Env", "dev", true, "Environment name")]);
        ValidateTemplateQuery? captured = null;
        _sender
            .Send(Arg.Do<ValidateTemplateQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ValidateTemplateQueryResult>>(
                new ValidateTemplateQueryResult(validation)));
        var sut = CreateSut();

        // Act
        var result = await sut.ValidateTemplate(request, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CloudFormationTemplateValidationResponse>>().Subject;
        ok.Value!.Description.Should().Be("An example template");
        ok.Value.CapabilitiesReason.Should().Be("Requires IAM");
        ok.Value.Capabilities.Should().ContainSingle().Which.Should().Be("CAPABILITY_IAM");
        var parameter = ok.Value.Parameters.Should().ContainSingle().Subject;
        parameter.ParameterKey.Should().Be("Env");
        parameter.DefaultValue.Should().Be("dev");
        parameter.NoEcho.Should().BeTrue();
        parameter.Description.Should().Be("Environment name");
        captured.Should().NotBeNull();
        captured!.TemplateBody.Should().Be("{\"Resources\":{}}");
        captured.TemplateUrl.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTemplate_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new CloudFormationTemplateValidationRequest(null, "https://example.s3.amazonaws.com/template.json");
        _sender
            .Send(Arg.Any<ValidateTemplateQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ValidateTemplateQueryResult>>(new Error("invalid template")));
        var sut = CreateSut();

        // Act
        var result = await sut.ValidateTemplate(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateStack_WhenCommandSucceeds_ReturnsOkWithStackIdAndForwardsRequest()
    {
        // Arrange
        const string name = "orders-stack";
        var request = new CloudFormationStackUpdateRequest(
            "{\"Resources\":{}}",
            [new("Env", "prod")],
            ["CAPABILITY_NAMED_IAM"]);
        UpdateStackCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateStackCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(
                "arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc"));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateStack(name, request, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CloudFormationStackOperationResponse>>().Subject;
        ok.Value!.StackId.Should()
            .Be("arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc");
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be("orders-stack");
        captured.TemplateBody.Should().Be("{\"Resources\":{}}");
        var parameter = captured.Parameters.Should().ContainSingle().Subject;
        parameter.ParameterKey.Should().Be("Env");
        parameter.ParameterValue.Should().Be("prod");
        captured.Capabilities.Should().ContainSingle().Which.Should().Be("CAPABILITY_NAMED_IAM");
    }

    [Fact]
    public async Task UpdateStack_WhenRequestOmitsCollections_DefaultsToEmpty()
    {
        // Arrange
        var request = new CloudFormationStackUpdateRequest("{}", null, null);
        UpdateStackCommand? captured = null;
        _sender
            .Send(Arg.Do<UpdateStackCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("arn:stack"));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateStack(
            "orders-stack", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Ok<CloudFormationStackOperationResponse>>();
        captured.Should().NotBeNull();
        captured!.Parameters.Should().BeEmpty();
        captured.Capabilities.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateStack_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new CloudFormationStackUpdateRequest("{}", null, null);
        _sender
            .Send(Arg.Any<UpdateStackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateStack(
            "orders-stack", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteStack_WhenCommandSucceeds_ReturnsNoContentAndForwardsName()
    {
        // Arrange
        const string name = "orders-stack";
        DeleteStackCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteStackCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteStack(name, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be("orders-stack");
    }

    [Fact]
    public async Task DeleteStack_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteStackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteStack("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListChangeSets_WhenQuerySucceeds_ReturnsOkWithChangeSetsAndForwardsName()
    {
        // Arrange
        const string name = "orders-stack";
        IReadOnlyList<ChangeSetSummary> changeSets =
        [
            new(
                "arn:changeset/add-queue",
                "add-queue",
                name,
                "CREATE_COMPLETE",
                "Ready",
                "AVAILABLE",
                "Adds a queue",
                DateTime.UnixEpoch),
        ];
        ListChangeSetsQuery? captured = null;
        _sender
            .Send(Arg.Do<ListChangeSetsQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListChangeSetsQueryResult>>(
                new ListChangeSetsQueryResult(changeSets)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListChangeSets(name, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CloudFormationChangeSetListResponse>>().Subject;
        var changeSet = ok.Value!.ChangeSets.Should().ContainSingle().Subject;
        changeSet.ChangeSetId.Should().Be("arn:changeset/add-queue");
        changeSet.ChangeSetName.Should().Be("add-queue");
        changeSet.StackName.Should().Be("orders-stack");
        changeSet.Status.Should().Be("CREATE_COMPLETE");
        changeSet.StatusReason.Should().Be("Ready");
        changeSet.ExecutionStatus.Should().Be("AVAILABLE");
        changeSet.Description.Should().Be("Adds a queue");
        changeSet.CreationTime.Should().Be(DateTime.UnixEpoch);
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be(name);
    }

    [Fact]
    public async Task ListChangeSets_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListChangeSetsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListChangeSetsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListChangeSets("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DescribeChangeSet_WhenQuerySucceeds_ReturnsOkWithDetailsAndForwardsArguments()
    {
        // Arrange
        const string name = "orders-stack";
        const string changeSetName = "add-queue";
        var detail = new ChangeSetDetail(
            changeSetName,
            "arn:changeset/add-queue",
            name,
            "arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc",
            "CREATE_COMPLETE",
            "Ready",
            "AVAILABLE",
            "Adds a queue",
            DateTime.UnixEpoch,
            [new("Env", "dev")],
            ["CAPABILITY_IAM"],
            [new("Add", "OrdersQueue", "orders-queue", "AWS::SQS::Queue", "False")]);
        DescribeChangeSetQuery? captured = null;
        _sender
            .Send(Arg.Do<DescribeChangeSetQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DescribeChangeSetQueryResult>>(
                new DescribeChangeSetQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.DescribeChangeSet(
            name, changeSetName, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CloudFormationChangeSetDetailResponse>>().Subject;
        ok.Value!.ChangeSetName.Should().Be("add-queue");
        ok.Value.ChangeSetId.Should().Be("arn:changeset/add-queue");
        ok.Value.StackName.Should().Be("orders-stack");
        ok.Value.StackId.Should()
            .Be("arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc");
        ok.Value.Status.Should().Be("CREATE_COMPLETE");
        ok.Value.StatusReason.Should().Be("Ready");
        ok.Value.ExecutionStatus.Should().Be("AVAILABLE");
        ok.Value.Description.Should().Be("Adds a queue");
        ok.Value.CreationTime.Should().Be(DateTime.UnixEpoch);
        var parameter = ok.Value.Parameters.Should().ContainSingle().Subject;
        parameter.ParameterKey.Should().Be("Env");
        parameter.ParameterValue.Should().Be("dev");
        ok.Value.Capabilities.Should().ContainSingle().Which.Should().Be("CAPABILITY_IAM");
        var change = ok.Value.Changes.Should().ContainSingle().Subject;
        change.Action.Should().Be("Add");
        change.LogicalResourceId.Should().Be("OrdersQueue");
        change.PhysicalResourceId.Should().Be("orders-queue");
        change.ResourceType.Should().Be("AWS::SQS::Queue");
        change.Replacement.Should().Be("False");
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be(name);
        captured.ChangeSetName.Should().Be(changeSetName);
    }

    [Fact]
    public async Task DescribeChangeSet_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DescribeChangeSetQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DescribeChangeSetQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DescribeChangeSet(
            "missing", "add-queue", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateChangeSet_WhenCommandSucceeds_ReturnsCreatedWithIdAndForwardsRequest()
    {
        // Arrange
        var request = new CloudFormationChangeSetCreateRequest(
            "orders-stack",
            "add-queue",
            "UPDATE",
            "{\"Resources\":{}}",
            [new("Env", "dev")],
            ["CAPABILITY_IAM"]);
        CreateChangeSetCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateChangeSetCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("arn:changeset/add-queue"));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateChangeSet(request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should()
            .BeOfType<Created<CloudFormationChangeSetOperationResponse>>().Subject;
        created.Value!.ChangeSetId.Should().Be("arn:changeset/add-queue");
        created.Location.Should()
            .Be("/api/services/cloudformation/change-set?name=orders-stack&changeSet=add-queue");
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be("orders-stack");
        captured.ChangeSetName.Should().Be("add-queue");
        captured.ChangeSetType.Should().Be("UPDATE");
        captured.TemplateBody.Should().Be("{\"Resources\":{}}");
        var parameter = captured.Parameters.Should().ContainSingle().Subject;
        parameter.ParameterKey.Should().Be("Env");
        parameter.ParameterValue.Should().Be("dev");
        captured.Capabilities.Should().ContainSingle().Which.Should().Be("CAPABILITY_IAM");
    }

    [Fact]
    public async Task CreateChangeSet_WhenRequestOmitsCollections_DefaultsToEmpty()
    {
        // Arrange
        var request = new CloudFormationChangeSetCreateRequest(
            "orders-stack",
            "add-queue",
            "CREATE",
            "{\"Resources\":{}}",
            null,
            null);
        CreateChangeSetCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateChangeSetCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("arn:changeset/add-queue"));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateChangeSet(request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Created<CloudFormationChangeSetOperationResponse>>();
        captured.Should().NotBeNull();
        captured!.Parameters.Should().BeEmpty();
        captured.Capabilities.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateChangeSet_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        var request = new CloudFormationChangeSetCreateRequest(
            "orders-stack", "add-queue", "UPDATE", "{}", null, null);
        _sender
            .Send(Arg.Any<CreateChangeSetCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateChangeSet(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ExecuteChangeSet_WhenCommandSucceeds_ReturnsNoContentAndForwardsArguments()
    {
        // Arrange
        const string name = "orders-stack";
        const string changeSetName = "add-queue";
        ExecuteChangeSetCommand? captured = null;
        _sender
            .Send(Arg.Do<ExecuteChangeSetCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteChangeSet(
            name, changeSetName, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be("orders-stack");
        captured.ChangeSetName.Should().Be("add-queue");
    }

    [Fact]
    public async Task ExecuteChangeSet_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ExecuteChangeSetCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteChangeSet(
            "missing", "add-queue", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteChangeSet_WhenCommandSucceeds_ReturnsNoContentAndForwardsArguments()
    {
        // Arrange
        const string name = "orders-stack";
        const string changeSetName = "add-queue";
        DeleteChangeSetCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteChangeSetCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteChangeSet(
            name, changeSetName, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be("orders-stack");
        captured.ChangeSetName.Should().Be("add-queue");
    }

    [Fact]
    public async Task DeleteChangeSet_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteChangeSetCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteChangeSet(
            "missing", "add-queue", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DetectStackDrift_WhenCommandSucceeds_ReturnsAcceptedWithIdAndForwardsArguments()
    {
        // Arrange
        const string name = "orders-stack";
        DetectStackDriftCommand? captured = null;
        _sender
            .Send(Arg.Do<DetectStackDriftCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("drift-123"));
        var sut = CreateSut();

        // Act
        var result = await sut.DetectStackDrift(name, TestContext.Current.CancellationToken);

        // Assert
        var accepted = result.Should()
            .BeOfType<Accepted<CloudFormationDriftDetectionResponse>>().Subject;
        accepted.Value!.StackDriftDetectionId.Should().Be("drift-123");
        accepted.Location.Should()
            .Be("/api/services/cloudformation/stack/drift?driftDetectionId=drift-123");
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be("orders-stack");
    }

    [Fact]
    public async Task DetectStackDrift_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DetectStackDriftCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DetectStackDrift("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetDriftStatus_WhenQuerySucceeds_ReturnsOkWithStatusAndForwardsArguments()
    {
        // Arrange
        const string driftDetectionId = "drift-123";
        var status = new StackDriftStatus(
            driftDetectionId,
            "arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc",
            "DETECTION_COMPLETE",
            "Done",
            "DRIFTED",
            2,
            DateTime.UnixEpoch);
        GetDriftStatusQuery? captured = null;
        _sender
            .Send(Arg.Do<GetDriftStatusQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetDriftStatusQueryResult>>(
                new GetDriftStatusQueryResult(status)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetDriftStatus(
            driftDetectionId, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CloudFormationDriftStatusResponse>>().Subject;
        ok.Value!.StackDriftDetectionId.Should().Be("drift-123");
        ok.Value.StackId.Should()
            .Be("arn:aws:cloudformation:eu-west-1:000000000000:stack/orders-stack/abc");
        ok.Value.DetectionStatus.Should().Be("DETECTION_COMPLETE");
        ok.Value.DetectionStatusReason.Should().Be("Done");
        ok.Value.StackDriftStatus.Should().Be("DRIFTED");
        ok.Value.DriftedStackResourceCount.Should().Be(2);
        ok.Value.Timestamp.Should().Be(DateTime.UnixEpoch);
        captured.Should().NotBeNull();
        captured!.DriftDetectionId.Should().Be(driftDetectionId);
    }

    [Fact]
    public async Task GetDriftStatus_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetDriftStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetDriftStatusQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetDriftStatus("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListResourceDrifts_WhenQuerySucceeds_ReturnsOkWithDriftsAndForwardsArguments()
    {
        // Arrange
        const string name = "orders-stack";
        IReadOnlyList<StackResourceDrift> drifts =
        [
            new(
                "OrdersQueue",
                "orders-queue",
                "AWS::SQS::Queue",
                "MODIFIED",
                "{\"DelaySeconds\":\"0\"}",
                "{\"DelaySeconds\":\"30\"}",
                DateTime.UnixEpoch),
        ];
        ListResourceDriftsQuery? captured = null;
        _sender
            .Send(Arg.Do<ListResourceDriftsQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListResourceDriftsQueryResult>>(
                new ListResourceDriftsQueryResult(drifts)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListResourceDrifts(name, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CloudFormationResourceDriftListResponse>>().Subject;
        var drift = ok.Value!.Drifts.Should().ContainSingle().Subject;
        drift.LogicalResourceId.Should().Be("OrdersQueue");
        drift.PhysicalResourceId.Should().Be("orders-queue");
        drift.ResourceType.Should().Be("AWS::SQS::Queue");
        drift.DriftStatus.Should().Be("MODIFIED");
        drift.ExpectedProperties.Should().Be("{\"DelaySeconds\":\"0\"}");
        drift.ActualProperties.Should().Be("{\"DelaySeconds\":\"30\"}");
        drift.Timestamp.Should().Be(DateTime.UnixEpoch);
        captured.Should().NotBeNull();
        captured!.StackName.Should().Be(name);
    }

    [Fact]
    public async Task ListResourceDrifts_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListResourceDriftsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListResourceDriftsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListResourceDrifts("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListExports_WhenQuerySucceeds_ReturnsOkWithExports()
    {
        // Arrange
        IReadOnlyList<StackExport> exports =
        [
            new("shared-vpc-id", "vpc-12345", "arn:aws:cloudformation:eu-west-1:000000000000:stack/network/abc"),
        ];
        _sender
            .Send(Arg.Any<ListExportsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListExportsQueryResult>>(
                new ListExportsQueryResult(exports)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListExports(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CloudFormationExportListResponse>>().Subject;
        var export = ok.Value!.Exports.Should().ContainSingle().Subject;
        export.Name.Should().Be("shared-vpc-id");
        export.Value.Should().Be("vpc-12345");
        export.ExportingStackId.Should().Be("arn:aws:cloudformation:eu-west-1:000000000000:stack/network/abc");
    }

    [Fact]
    public async Task ListExports_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListExportsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListExportsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListExports(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListImports_WhenQuerySucceeds_ReturnsOkWithStacksAndForwardsArguments()
    {
        // Arrange
        const string exportName = "shared-vpc-id";
        IReadOnlyList<string> stacks = ["orders-stack", "billing-stack"];
        ListImportsQuery? captured = null;
        _sender
            .Send(Arg.Do<ListImportsQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListImportsQueryResult>>(
                new ListImportsQueryResult(stacks)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListImports(exportName, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<CloudFormationImportListResponse>>().Subject;
        ok.Value!.ImportingStackNames.Should().BeEquivalentTo(stacks);
        captured.Should().NotBeNull();
        captured!.ExportName.Should().Be(exportName);
    }

    [Fact]
    public async Task ListImports_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListImportsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListImportsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListImports("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
