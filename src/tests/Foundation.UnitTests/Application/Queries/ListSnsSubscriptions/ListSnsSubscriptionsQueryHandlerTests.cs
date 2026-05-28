using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListSnsSubscriptions;
using Foundation.Application.Sns;
using Foundation.Domain.Sns;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListSnsSubscriptions;

public class ListSnsSubscriptionsQueryHandlerTests
{
    private readonly ISnsClient _client = Substitute.For<ISnsClient>();

    private ListSnsSubscriptionsQueryHandler CreateSut()
        => new(_client, NullLogger<ListSnsSubscriptionsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsSubscriptions()
    {
        // Arrange
        IReadOnlyList<SnsSubscription> subscriptions =
        [
            new(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f...",
                "sqs",
                "arn:aws:sqs:eu-west-1:000000000000:orders",
                "000000000000"),
        ];
        _client
            .ListSubscriptionsByTopicAsync(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(subscriptions)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSnsSubscriptionsQuery("arn:aws:sns:eu-west-1:000000000000:orders-topic"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Subscriptions.Should().ContainSingle(_ => _.Protocol == "sqs");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListSubscriptionsByTopicAsync(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SnsSubscription>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSnsSubscriptionsQuery("arn:aws:sns:eu-west-1:000000000000:orders-topic"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
