using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateLambdaFunction;
using Foundation.Application.Commands.DeleteLambdaFunction;
using Foundation.Application.Commands.DeleteLambdaTestEvent;
using Foundation.Application.Commands.InvokeLambdaFunction;
using Foundation.Application.Commands.SaveLambdaTestEvent;
using Foundation.Application.Commands.SetLambdaEventSourceMappingState;
using Foundation.Application.Commands.UpdateLambdaEnvironment;
using Foundation.Application.Commands.UpdateLambdaFunction;
using Foundation.Application.Queries.GetLambdaEnvironment;
using Foundation.Application.Queries.GetLambdaFunction;
using Foundation.Application.Queries.GetLambdaInvocationInsights;
using Foundation.Application.Queries.ListLambdaEventSourceMappings;
using Foundation.Application.Queries.ListLambdaFunctions;
using Foundation.Application.Queries.ListLambdaLayers;
using Foundation.Application.Queries.ListLambdaLogEvents;
using Foundation.Application.Queries.ListLambdaTestEvents;
using Foundation.Domain.Lambda;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class LambdaControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<LambdaController> _logger = Substitute.For<ILogger<LambdaController>>();

    private LambdaController CreateSut()
        => new(_sender, _logger);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task ListFunctions_WhenQuerySucceeds_ReturnsOkWithSummaries()
    {
        // Arrange
        IReadOnlyList<LambdaFunctionSummary> functions =
        [
            new("process-orders", "dotnet8", "Order processor", "2026-01-02T03:04:05Z", 256, 30),
        ];
        _sender
            .Send(Arg.Any<ListLambdaFunctionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLambdaFunctionsQueryResult>>(
                new ListLambdaFunctionsQueryResult(functions)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListFunctions(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<LambdaFunctionListResponse>>().Subject;
        var summary = ok.Value!.Functions.Should().ContainSingle().Subject;
        summary.FunctionName.Should().Be("process-orders");
        summary.Runtime.Should().Be("dotnet8");
        summary.Description.Should().Be("Order processor");
        summary.LastModified.Should().Be("2026-01-02T03:04:05Z");
        summary.MemorySize.Should().Be(256);
        summary.Timeout.Should().Be(30);
    }

    [Fact]
    public async Task ListFunctions_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListLambdaFunctionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLambdaFunctionsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListFunctions(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetFunction_WhenQuerySucceeds_ReturnsOkWithDetail()
    {
        // Arrange
        var detail = new LambdaFunctionDetail(
            "process-orders",
            "arn:aws:lambda:eu-west-1:000000000000:function:process-orders",
            "dotnet8",
            "Orders::Handler",
            "Order processor",
            "2026-01-02T03:04:05Z",
            256,
            30,
            "arn:aws:iam::000000000000:role/lambda-orders");
        _sender
            .Send(Arg.Any<GetLambdaFunctionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLambdaFunctionQueryResult>>(
                new GetLambdaFunctionQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetFunction("process-orders", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<LambdaFunctionResponse>>().Subject;
        ok.Value!.FunctionName.Should().Be("process-orders");
        ok.Value.FunctionArn.Should().Be("arn:aws:lambda:eu-west-1:000000000000:function:process-orders");
        ok.Value.Runtime.Should().Be("dotnet8");
        ok.Value.Handler.Should().Be("Orders::Handler");
        ok.Value.Description.Should().Be("Order processor");
        ok.Value.LastModified.Should().Be("2026-01-02T03:04:05Z");
        ok.Value.MemorySize.Should().Be(256);
        ok.Value.Timeout.Should().Be(30);
        ok.Value.Role.Should().Be("arn:aws:iam::000000000000:role/lambda-orders");
    }

    [Fact]
    public async Task GetFunction_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetLambdaFunctionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLambdaFunctionQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetFunction("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetEnvironment_WhenQuerySucceeds_ReturnsOkWithVariables()
    {
        // Arrange
        IReadOnlyList<LambdaEnvironmentVariable> variables =
        [
            new("API_KEY", "********", true),
            new("REGION", "eu-west-1", false),
        ];
        _sender
            .Send(Arg.Any<GetLambdaEnvironmentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLambdaEnvironmentQueryResult>>(
                new GetLambdaEnvironmentQueryResult(variables, true)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetEnvironment("orders", false, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<LambdaEnvironmentResponse>>().Subject;
        ok.Value!.RevealAllowed.Should().BeTrue();
        ok.Value.Variables.Should().HaveCount(2);
        var first = ok.Value.Variables[0];
        first.Name.Should().Be("API_KEY");
        first.Value.Should().Be("********");
        first.IsSensitive.Should().BeTrue();
        var second = ok.Value.Variables[1];
        second.Name.Should().Be("REGION");
        second.Value.Should().Be("eu-west-1");
        second.IsSensitive.Should().BeFalse();
    }

    [Fact]
    public async Task GetEnvironment_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetLambdaEnvironmentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLambdaEnvironmentQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetEnvironment("missing", false, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateEnvironment_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateLambdaEnvironmentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var request = new LambdaEnvironmentUpdateRequest(
        [
            new("REGION", "eu-west-1"),
            new("REGION", "us-east-1"),
        ]);
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateEnvironment("orders", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<UpdateLambdaEnvironmentCommand>(command =>
                command.FunctionName == "orders"
                && command.Variables["REGION"] == "us-east-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateEnvironment_WhenVariablesNull_SendsEmptyDictionary()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateLambdaEnvironmentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var request = new LambdaEnvironmentUpdateRequest(null!);
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateEnvironment("orders", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<UpdateLambdaEnvironmentCommand>(command => command.Variables.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateEnvironment_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateLambdaEnvironmentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var request = new LambdaEnvironmentUpdateRequest([new("REGION", "eu-west-1")]);
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateEnvironment("orders", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task Invoke_WhenCommandSucceeds_ReturnsOkWithInvocationResult()
    {
        // Arrange
        var invocation = new LambdaInvocationResult(200, "{\"ok\":true}", "REPORT Billed Duration: 12 ms", "Unhandled", 18);
        _sender
            .Send(Arg.Any<InvokeLambdaFunctionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(invocation)));
        var sut = CreateSut();

        // Act
        var result = await sut.Invoke("orders", new LambdaInvokeRequest("{\"id\":1}"), TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<LambdaInvocationResponse>>().Subject;
        ok.Value!.StatusCode.Should().Be(200);
        ok.Value.Payload.Should().Be("{\"ok\":true}");
        ok.Value.LogTail.Should().Be("REPORT Billed Duration: 12 ms");
        ok.Value.FunctionError.Should().Be("Unhandled");
        ok.Value.DurationMs.Should().Be(18);
        await _sender.Received(1).Send(
            Arg.Is<InvokeLambdaFunctionCommand>(command =>
                command.FunctionName == "orders" && command.Payload == "{\"id\":1}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Invoke_WhenPayloadNull_SendsEmptyPayload()
    {
        // Arrange
        var invocation = new LambdaInvocationResult(200, "{}", string.Empty, string.Empty, 1);
        _sender
            .Send(Arg.Any<InvokeLambdaFunctionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(invocation)));
        var sut = CreateSut();

        // Act
        var result = await sut.Invoke("orders", new LambdaInvokeRequest(null), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Ok<LambdaInvocationResponse>>();
        await _sender.Received(1).Send(
            Arg.Is<InvokeLambdaFunctionCommand>(command => command.Payload == string.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Invoke_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<InvokeLambdaFunctionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LambdaInvocationResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Invoke("orders", new LambdaInvokeRequest("{}"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateFunction_WhenCommandSucceeds_ReturnsCreated()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateLambdaFunctionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var request = new LambdaFunctionCreateRequest(
            "orders", "python3.12", "index.handler", "arn:role", "Order processor", 256, 30, "QkFTRTY0");
        var sut = CreateSut();

        // Act
        var result = await sut.CreateFunction(request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created>().Subject;
        created.Location.Should().Be("/api/services/lambda/functions/orders");
        await _sender.Received(1).Send(
            Arg.Is<CreateLambdaFunctionCommand>(command =>
                command.FunctionName == "orders"
                && command.Runtime == "python3.12"
                && command.Handler == "index.handler"
                && command.Role == "arn:role"
                && command.Description == "Order processor"
                && command.MemorySize == 256
                && command.Timeout == 30
                && command.ZipFileBase64 == "QkFTRTY0"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateFunction_WhenDescriptionNull_SendsEmptyDescription()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateLambdaFunctionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var request = new LambdaFunctionCreateRequest(
            "orders", "python3.12", "index.handler", "arn:role", null, 256, 30, "QkFTRTY0");
        var sut = CreateSut();

        // Act
        var result = await sut.CreateFunction(request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Created>();
        await _sender.Received(1).Send(
            Arg.Is<CreateLambdaFunctionCommand>(command => command.Description == string.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateFunction_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateLambdaFunctionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var request = new LambdaFunctionCreateRequest(
            "orders", "python3.12", "index.handler", "arn:role", "desc", 256, 30, "QkFTRTY0");
        var sut = CreateSut();

        // Act
        var result = await sut.CreateFunction(request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateFunction_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateLambdaFunctionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var request = new LambdaFunctionUpdateRequest(
            "python3.12", "index.handler", "arn:role", "Order processor", 256, 30, "QkFTRTY0");
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateFunction("orders", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<UpdateLambdaFunctionCommand>(command =>
                command.FunctionName == "orders"
                && command.Runtime == "python3.12"
                && command.Handler == "index.handler"
                && command.Role == "arn:role"
                && command.Description == "Order processor"
                && command.MemorySize == 256
                && command.Timeout == 30
                && command.ZipFileBase64 == "QkFTRTY0"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateFunction_WhenDescriptionNull_SendsEmptyDescription()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateLambdaFunctionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var request = new LambdaFunctionUpdateRequest(
            "python3.12", "index.handler", "arn:role", null, 256, 30, null);
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateFunction("orders", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<UpdateLambdaFunctionCommand>(command =>
                command.Description == string.Empty && command.ZipFileBase64 == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateFunction_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateLambdaFunctionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var request = new LambdaFunctionUpdateRequest(
            "python3.12", "index.handler", "arn:role", "desc", 256, 30, null);
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateFunction("orders", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteFunction_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteLambdaFunctionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteFunction("orders", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<DeleteLambdaFunctionCommand>(command => command.FunctionName == "orders"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteFunction_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteLambdaFunctionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteFunction("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListTestEvents_WhenQuerySucceeds_ReturnsOkWithEventsAndTemplates()
    {
        // Arrange
        IReadOnlyList<LambdaTestEvent> events = [new("saved", "{\"a\":1}")];
        IReadOnlyList<LambdaTestEvent> templates = [new("Empty", "{}")];
        _sender
            .Send(Arg.Any<ListLambdaTestEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLambdaTestEventsQueryResult>>(
                new ListLambdaTestEventsQueryResult(events, templates)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListTestEvents("orders", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<LambdaTestEventListResponse>>().Subject;
        var savedEvent = ok.Value!.Events.Should().ContainSingle().Subject;
        savedEvent.Name.Should().Be("saved");
        savedEvent.Payload.Should().Be("{\"a\":1}");
        var template = ok.Value!.Templates.Should().ContainSingle().Subject;
        template.Name.Should().Be("Empty");
        template.Payload.Should().Be("{}");
        await _sender.Received(1).Send(
            Arg.Is<ListLambdaTestEventsQuery>(query => query.FunctionName == "orders"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListTestEvents_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListLambdaTestEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLambdaTestEventsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListTestEvents("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task SaveTestEvent_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SaveLambdaTestEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.SaveTestEvent(
            "orders",
            new LambdaTestEventSaveRequest("first", "{\"a\":1}"),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<SaveLambdaTestEventCommand>(command =>
                command.FunctionName == "orders" && command.Name == "first" && command.Payload == "{\"a\":1}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveTestEvent_WhenPayloadNull_DefaultsToEmptyObject()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SaveLambdaTestEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.SaveTestEvent(
            "orders",
            new LambdaTestEventSaveRequest("first", null),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<SaveLambdaTestEventCommand>(command => command.Payload == "{}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveTestEvent_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SaveLambdaTestEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.SaveTestEvent(
            "orders",
            new LambdaTestEventSaveRequest("first", "{}"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteTestEvent_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteLambdaTestEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteTestEvent("orders", "first", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<DeleteLambdaTestEventCommand>(command =>
                command.FunctionName == "orders" && command.Name == "first"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteTestEvent_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteLambdaTestEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteTestEvent("orders", "first", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListEventSourceMappings_WhenQuerySucceeds_ReturnsOkWithMappings()
    {
        // Arrange
        IReadOnlyList<LambdaEventSourceMapping> mappings =
        [
            new("uuid-1", "arn:source", "arn:fn", "Enabled", 10, "2026-01-02T03:04:05Z"),
        ];
        IReadOnlyList<LambdaS3Trigger> triggers =
        [
            new("arn:aws:s3:::orders-bucket"),
        ];
        _sender
            .Send(Arg.Any<ListLambdaEventSourceMappingsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLambdaEventSourceMappingsQueryResult>>(
                new ListLambdaEventSourceMappingsQueryResult(mappings, triggers)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListEventSourceMappings("orders", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<LambdaEventSourceMappingListResponse>>().Subject;
        var mapping = ok.Value!.Mappings.Should().ContainSingle().Subject;
        mapping.Uuid.Should().Be("uuid-1");
        mapping.EventSourceArn.Should().Be("arn:source");
        mapping.FunctionArn.Should().Be("arn:fn");
        mapping.State.Should().Be("Enabled");
        mapping.BatchSize.Should().Be(10);
        mapping.LastModified.Should().Be("2026-01-02T03:04:05Z");
        ok.Value!.S3Triggers.Should().ContainSingle().Which.BucketArn.Should().Be("arn:aws:s3:::orders-bucket");
        await _sender.Received(1).Send(
            Arg.Is<ListLambdaEventSourceMappingsQuery>(query => query.FunctionName == "orders"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListEventSourceMappings_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListLambdaEventSourceMappingsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLambdaEventSourceMappingsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListEventSourceMappings("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task SetEventSourceMappingState_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SetLambdaEventSourceMappingStateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.SetEventSourceMappingState(
            "orders",
            "uuid-1",
            new LambdaEventSourceMappingStateRequest(false),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<SetLambdaEventSourceMappingStateCommand>(command =>
                command.FunctionName == "orders" && command.Uuid == "uuid-1" && !command.Enabled),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetEventSourceMappingState_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SetLambdaEventSourceMappingStateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.SetEventSourceMappingState(
            "orders",
            "uuid-1",
            new LambdaEventSourceMappingStateRequest(true),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListLogEvents_WhenQuerySucceeds_ReturnsOkWithLogEvents()
    {
        // Arrange
        IReadOnlyList<LambdaLogEvent> events =
        [
            new("2026-01-02T03:04:05.0000000+00:00", "hello", "stream-a"),
        ];
        _sender
            .Send(Arg.Any<ListLambdaLogEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLambdaLogEventsQueryResult>>(
                new ListLambdaLogEventsQueryResult("/aws/lambda/orders", events)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListLogEvents("orders", 50, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<LambdaLogEventListResponse>>().Subject;
        ok.Value!.LogGroupName.Should().Be("/aws/lambda/orders");
        var logEvent = ok.Value!.Events.Should().ContainSingle().Subject;
        logEvent.Timestamp.Should().Be("2026-01-02T03:04:05.0000000+00:00");
        logEvent.Message.Should().Be("hello");
        logEvent.LogStreamName.Should().Be("stream-a");
        await _sender.Received(1).Send(
            Arg.Is<ListLambdaLogEventsQuery>(query => query.FunctionName == "orders" && query.Limit == 50),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListLogEvents_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListLambdaLogEventsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLambdaLogEventsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListLogEvents("orders", 50, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetInvocationInsights_WhenQuerySucceeds_ReturnsOkWithInsights()
    {
        // Arrange
        var insights = new LambdaInvocationInsights(
            new LambdaInvocationMetrics(2, 1, 15.0, 30.0),
            [new LambdaRecentInvocation("abc", "2026-01-02T03:04:05.0000000+00:00", 30.0, true)]);
        _sender
            .Send(Arg.Any<GetLambdaInvocationInsightsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLambdaInvocationInsightsQueryResult>>(
                new GetLambdaInvocationInsightsQueryResult("/aws/lambda/orders", insights)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetInvocationInsights("orders", 50, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<LambdaInvocationInsightsResponse>>().Subject;
        ok.Value!.LogGroupName.Should().Be("/aws/lambda/orders");
        ok.Value!.Metrics.InvocationCount.Should().Be(2);
        ok.Value!.Metrics.ErrorCount.Should().Be(1);
        ok.Value!.Metrics.AverageDurationMs.Should().Be(15.0);
        ok.Value!.Metrics.MaxDurationMs.Should().Be(30.0);
        var invocation = ok.Value!.RecentInvocations.Should().ContainSingle().Subject;
        invocation.RequestId.Should().Be("abc");
        invocation.Timestamp.Should().Be("2026-01-02T03:04:05.0000000+00:00");
        invocation.DurationMs.Should().Be(30.0);
        invocation.HasError.Should().BeTrue();
        await _sender.Received(1).Send(
            Arg.Is<GetLambdaInvocationInsightsQuery>(query => query.FunctionName == "orders" && query.Limit == 50),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetInvocationInsights_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetLambdaInvocationInsightsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetLambdaInvocationInsightsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetInvocationInsights("orders", 50, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListLayers_WhenQuerySucceeds_ReturnsOkWithLayers()
    {
        // Arrange
        IReadOnlyList<LambdaLayer> layers =
        [
            new("arn:aws:lambda:eu-west-1:123456789012:layer:shared-utils:7", "shared-utils", "7"),
        ];
        _sender
            .Send(Arg.Any<ListLambdaLayersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLambdaLayersQueryResult>>(
                new ListLambdaLayersQueryResult(layers)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListLayers("orders", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<LambdaLayerListResponse>>().Subject;
        var layer = ok.Value!.Layers.Should().ContainSingle().Subject;
        layer.Arn.Should().Be("arn:aws:lambda:eu-west-1:123456789012:layer:shared-utils:7");
        layer.Name.Should().Be("shared-utils");
        layer.Version.Should().Be("7");
        await _sender.Received(1).Send(
            Arg.Is<ListLambdaLayersQuery>(query => query.FunctionName == "orders"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListLayers_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListLambdaLayersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListLambdaLayersQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListLayers("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
