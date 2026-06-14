using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateSnsTopic;
using Foundation.Application.Commands.DeleteSnsTopic;
using Foundation.Application.Commands.PublishSnsMessage;
using Foundation.Application.Commands.SetSnsSubscriptionFilterPolicy;
using Foundation.Application.Commands.SubscribeSnsTopic;
using Foundation.Application.Commands.UnsubscribeSnsTopic;
using Foundation.Application.Queries.GetSnsSubscriptionFilterPolicy;
using Foundation.Application.Queries.ListSnsSubscriptions;
using Foundation.Application.Queries.ListSnsTopics;
using Foundation.Domain.Sns;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class SnsControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<SnsController> _logger = Substitute.For<ILogger<SnsController>>();

    private SnsController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListTopics_WhenQuerySucceeds_ReturnsOkWithTopics()
    {
        // Arrange
        IReadOnlyList<SnsTopic> topics =
        [
            new("orders-topic", "arn:aws:sns:eu-west-1:000000000000:orders-topic"),
        ];
        _sender
            .Send(Arg.Any<ListSnsTopicsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSnsTopicsQueryResult>>(
                new ListSnsTopicsQueryResult(topics)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListTopics(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SnsTopicListResponse>>().Subject;
        var topic = ok.Value!.Topics.Should().ContainSingle().Subject;
        topic.Name.Should().Be("orders-topic");
        topic.TopicArn.Should().Be("arn:aws:sns:eu-west-1:000000000000:orders-topic");
    }

    [Fact]
    public async Task ListTopics_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSnsTopicsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSnsTopicsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListTopics(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateTopic_WhenCommandSucceeds_ReturnsCreatedAndForwardsName()
    {
        // Arrange
        CreateSnsTopicCommand? captured = null;
        _sender
            .Send(Arg.Do<CreateSnsTopicCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateTopic(
            new SnsTopicCreateRequest("orders-topic"), TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("orders-topic");
    }

    [Fact]
    public async Task CreateTopic_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateSnsTopicCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateTopic(
            new SnsTopicCreateRequest("orders-topic"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteTopic_WhenCommandSucceeds_ReturnsNoContentAndForwardsArn()
    {
        // Arrange
        DeleteSnsTopicCommand? captured = null;
        _sender
            .Send(Arg.Do<DeleteSnsTopicCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteTopic(
            "arn:aws:sns:eu-west-1:000000000000:orders-topic", TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.TopicArn.Should().Be("arn:aws:sns:eu-west-1:000000000000:orders-topic");
    }

    [Fact]
    public async Task DeleteTopic_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteSnsTopicCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteTopic(
            "arn:aws:sns:eu-west-1:000000000000:orders-topic", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListSubscriptions_WhenQuerySucceeds_ReturnsOkWithSubscriptions()
    {
        // Arrange
        ListSnsSubscriptionsQuery? captured = null;
        IReadOnlyList<SnsSubscription> subscriptions =
        [
            new(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f",
                "sqs",
                "arn:aws:sqs:eu-west-1:000000000000:orders",
                "000000000000"),
        ];
        _sender
            .Send(Arg.Do<ListSnsSubscriptionsQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSnsSubscriptionsQueryResult>>(
                new ListSnsSubscriptionsQueryResult(subscriptions)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListSubscriptions(
            "arn:aws:sns:eu-west-1:000000000000:orders-topic", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SnsSubscriptionListResponse>>().Subject;
        var subscription = ok.Value!.Subscriptions.Should().ContainSingle().Subject;
        subscription.SubscriptionArn.Should().Be("arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f");
        subscription.Protocol.Should().Be("sqs");
        subscription.Endpoint.Should().Be("arn:aws:sqs:eu-west-1:000000000000:orders");
        subscription.Owner.Should().Be("000000000000");
        captured.Should().NotBeNull();
        captured!.TopicArn.Should().Be("arn:aws:sns:eu-west-1:000000000000:orders-topic");
    }

    [Fact]
    public async Task ListSubscriptions_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSnsSubscriptionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSnsSubscriptionsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListSubscriptions(
            "arn:aws:sns:eu-west-1:000000000000:orders-topic", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PublishMessage_WhenCommandSucceeds_ReturnsAcceptedAndForwardsArguments()
    {
        // Arrange
        PublishSnsMessageCommand? captured = null;
        _sender
            .Send(Arg.Do<PublishSnsMessageCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.PublishMessage(
            new SnsPublishMessageRequest(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic",
                "Subject",
                "hello",
                new Dictionary<string, string> { ["source"] = "test" }),
            TestContext.Current.CancellationToken);

        // Assert
        var accepted = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        accepted.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        captured.Should().NotBeNull();
        captured!.TopicArn.Should().Be("arn:aws:sns:eu-west-1:000000000000:orders-topic");
        captured.Subject.Should().Be("Subject");
        captured.Message.Should().Be("hello");
        captured.MessageAttributes["source"].Should().Be("test");
    }

    [Fact]
    public async Task PublishMessage_WhenAttributesNull_DefaultsToEmptyDictionary()
    {
        // Arrange
        PublishSnsMessageCommand? captured = null;
        _sender
            .Send(Arg.Do<PublishSnsMessageCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.PublishMessage(
            new SnsPublishMessageRequest(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic", null, "hello", null),
            TestContext.Current.CancellationToken);

        // Assert
        var accepted = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        accepted.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        captured.Should().NotBeNull();
        captured!.Subject.Should().BeNull();
        captured.MessageAttributes.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishMessage_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PublishSnsMessageCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PublishMessage(
            new SnsPublishMessageRequest(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic", null, "hello", null),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetFilterPolicy_WhenQuerySucceeds_ReturnsOkWithPolicyAndForwardsArn()
    {
        // Arrange
        GetSnsSubscriptionFilterPolicyQuery? captured = null;
        _sender
            .Send(
                Arg.Do<GetSnsSubscriptionFilterPolicyQuery>(query => captured = query),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSnsSubscriptionFilterPolicyQueryResult>>(
                new GetSnsSubscriptionFilterPolicyQueryResult("{\"store\":[\"x\"]}")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetFilterPolicy(
            "arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SnsSubscriptionFilterPolicyResponse>>().Subject;
        ok.Value!.FilterPolicy.Should().Be("{\"store\":[\"x\"]}");
        captured.Should().NotBeNull();
        captured!.SubscriptionArn.Should().Be("arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f");
    }

    [Fact]
    public async Task GetFilterPolicy_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetSnsSubscriptionFilterPolicyQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSnsSubscriptionFilterPolicyQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetFilterPolicy(
            "arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task SetFilterPolicy_WhenCommandSucceeds_ReturnsNoContentAndForwardsArguments()
    {
        // Arrange
        SetSnsSubscriptionFilterPolicyCommand? captured = null;
        _sender
            .Send(
                Arg.Do<SetSnsSubscriptionFilterPolicyCommand>(command => captured = command),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.SetFilterPolicy(
            new SnsSubscriptionFilterPolicyRequest(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f", "{\"store\":[\"x\"]}"),
            TestContext.Current.CancellationToken);

        // Assert
        var noContent = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        captured.Should().NotBeNull();
        captured!.SubscriptionArn.Should().Be("arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f");
        captured.FilterPolicy.Should().Be("{\"store\":[\"x\"]}");
    }

    [Fact]
    public async Task SetFilterPolicy_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SetSnsSubscriptionFilterPolicyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.SetFilterPolicy(
            new SnsSubscriptionFilterPolicyRequest(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f", "{}"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task Subscribe_WhenCommandSucceeds_ReturnsCreated()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SubscribeSnsTopicCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Subscribe(
            new SnsSubscribeRequest(
                "arn:aws:sns:eu-west-1:000000000000:topic",
                "sqs",
                "arn:aws:sqs:eu-west-1:000000000000:q"),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Created>();
        await _sender.Received(1).Send(
            Arg.Is<SubscribeSnsTopicCommand>(command =>
                command.Protocol == "sqs"
                && command.Endpoint == "arn:aws:sqs:eu-west-1:000000000000:q"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Subscribe_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SubscribeSnsTopicCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Subscribe(
            new SnsSubscribeRequest("arn:aws:sns:eu-west-1:000000000000:topic", "sqs", "e"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task Unsubscribe_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UnsubscribeSnsTopicCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Unsubscribe(
            "arn:aws:sns:eu-west-1:000000000000:topic:sub", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<UnsubscribeSnsTopicCommand>(command =>
                command.SubscriptionArn == "arn:aws:sns:eu-west-1:000000000000:topic:sub"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Unsubscribe_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UnsubscribeSnsTopicCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Unsubscribe(
            "arn:aws:sns:eu-west-1:000000000000:topic:sub", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
