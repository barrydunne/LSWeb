using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListSqsSubscriptions;
using Foundation.Application.Sqs;
using Foundation.Domain.Sqs;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListSqsSubscriptions;

public class ListSqsSubscriptionsQueryHandlerTests
{
    private readonly ISqsClient _client = Substitute.For<ISqsClient>();

    private ListSqsSubscriptionsQueryHandler CreateSut()
        => new(_client, NullLogger<ListSqsSubscriptionsQueryHandler>.Instance);

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
}
