using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListSqsSubscriptions;
using Foundation.Application.Sns;
using Foundation.Application.Sqs;
using Foundation.Domain.Sns;
using Foundation.Domain.Sqs;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListSqsSubscriptions;

public class ListSqsSubscriptionsQueryHandlerTests
{
    private readonly ISqsClient _client = Substitute.For<ISqsClient>();
    private readonly ISnsClient _snsClient = Substitute.For<ISnsClient>();

    public ListSqsSubscriptionsQueryHandlerTests()
    {
        _client
            .GetQueueSubscriptionsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<SqsQueueSubscription>>([])));
        _snsClient
            .ListTopicsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<SnsTopic>>([])));
        _snsClient
            .ListSubscriptionsByTopicAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<SnsSubscription>>([])));
    }

    private ListSqsSubscriptionsQueryHandler CreateSut()
        => new(_client, _snsClient, NullLogger<ListSqsSubscriptionsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsSubscriptions()
    {
        // Arrange
        IReadOnlyList<SqsQueueSubscription> subscriptions =
        [
            new("arn:aws:sns:eu-west-1:000000000000:order-events", "order-events"),
        ];
        _client
            .GetQueueSubscriptionsAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(subscriptions)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsSubscriptionsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Subscriptions.Should().ContainSingle(_ => _.TopicName == "order-events");
    }

    [Fact]
    public async Task Handle_WhenTopicSubscriptionTargetsQueue_IncludesSubscriptionEvenWhenPolicyHasNone()
    {
        // Arrange - the queue policy has no statements, but an SNS topic is subscribed to the queue.
        IReadOnlyList<SnsTopic> topics = [new("order-events", "arn:aws:sns:eu-west-1:000000000000:order-events")];
        _snsClient
            .ListTopicsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(topics)));
        _snsClient
            .ListSubscriptionsByTopicAsync(
                "arn:aws:sns:eu-west-1:000000000000:order-events", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<SnsSubscription>>(
            [
                new(
                    "arn:aws:sns:eu-west-1:000000000000:order-events:abc",
                    "sqs",
                    "arn:aws:sqs:eu-west-1:000000000000:orders",
                    "000000000000"),
            ])));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsSubscriptionsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Subscriptions.Select(_ => _.TopicArn)
              .Should().ContainSingle().Which.Should().Be("arn:aws:sns:eu-west-1:000000000000:order-events");
    }

    [Fact]
    public async Task Handle_WhenSubscriptionEndpointIsBareQueueName_IncludesTopic()
    {
        // Arrange - the subscription endpoint is the bare queue name rather than a full ARN.
        IReadOnlyList<SnsTopic> topics = [new("order-events", "arn:aws:sns:eu-west-1:000000000000:order-events")];
        _snsClient
            .ListTopicsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(topics)));
        _snsClient
            .ListSubscriptionsByTopicAsync(
                "arn:aws:sns:eu-west-1:000000000000:order-events", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<SnsSubscription>>(
            [
                new("arn:aws:sns:eu-west-1:000000000000:order-events:abc", "sqs", "orders", "000000000000"),
            ])));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsSubscriptionsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Subscriptions.Select(_ => _.TopicName)
              .Should().ContainSingle().Which.Should().Be("order-events");
    }

    [Fact]
    public async Task Handle_WhenSubscriptionDoesNotTargetQueue_ExcludesTopic()
    {
        // Arrange - non-matching subscriptions: wrong protocol, empty endpoint, a different queue
        // ARN, a non-SQS ARN endpoint, and a bare non-matching name. None resolve to the queue.
        IReadOnlyList<SnsTopic> topics = [new("noisy", "arn:aws:sns:eu-west-1:000000000000:noisy")];
        _snsClient
            .ListTopicsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(topics)));
        _snsClient
            .ListSubscriptionsByTopicAsync(
                "arn:aws:sns:eu-west-1:000000000000:noisy", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<SnsSubscription>>(
            [
                new("arn:1", "email", "team@example.com", "000000000000"),
                new("arn:2", "sqs", string.Empty, "000000000000"),
                new("arn:3", "sqs", "arn:aws:sqs:eu-west-1:000000000000:not-orders", "000000000000"),
                new("arn:4", "sqs", "arn:aws:lambda:eu-west-1:000000000000:function:orders", "000000000000"),
                new("arn:5", "sqs", "some-other-queue", "000000000000"),
            ])));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsSubscriptionsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Subscriptions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenSameTopicInPolicyAndSubscriptions_DeduplicatesAndOrders()
    {
        // Arrange - the same topic is discovered from both the queue policy and the SNS subscription;
        // it must appear once, and the results are ordered by topic ARN.
        IReadOnlyList<SqsQueueSubscription> policySubscriptions =
        [
            new("arn:aws:sns:eu-west-1:000000000000:shared", "shared"),
            new("arn:aws:sns:eu-west-1:000000000000:zeta", "zeta"),
        ];
        _client
            .GetQueueSubscriptionsAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(policySubscriptions)));
        IReadOnlyList<SnsTopic> topics =
        [
            new("shared", "arn:aws:sns:eu-west-1:000000000000:shared"),
            new("alpha", "arn:aws:sns:eu-west-1:000000000000:alpha"),
        ];
        _snsClient
            .ListTopicsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(topics)));
        _snsClient
            .ListSubscriptionsByTopicAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<SnsSubscription>>(
            [
                new("arn:sub", "sqs", "arn:aws:sqs:eu-west-1:000000000000:orders", "000000000000"),
            ])));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsSubscriptionsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Subscriptions.Select(_ => _.TopicArn).Should().Equal(
            "arn:aws:sns:eu-west-1:000000000000:alpha",
            "arn:aws:sns:eu-west-1:000000000000:shared",
            "arn:aws:sns:eu-west-1:000000000000:zeta");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetQueueSubscriptionsAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SqsQueueSubscription>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsSubscriptionsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }

    [Fact]
    public async Task Handle_WhenListTopicsFails_PropagatesError()
    {
        // Arrange
        _snsClient
            .ListTopicsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SnsTopic>>>(new Error("topics boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsSubscriptionsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("topics boom");
    }

    [Fact]
    public async Task Handle_WhenListSubscriptionsByTopicFails_PropagatesError()
    {
        // Arrange
        IReadOnlyList<SnsTopic> topics = [new("order-events", "arn:aws:sns:eu-west-1:000000000000:order-events")];
        _snsClient
            .ListTopicsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(topics)));
        _snsClient
            .ListSubscriptionsByTopicAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SnsSubscription>>>(new Error("subs boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSqsSubscriptionsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("subs boom");
    }
}
