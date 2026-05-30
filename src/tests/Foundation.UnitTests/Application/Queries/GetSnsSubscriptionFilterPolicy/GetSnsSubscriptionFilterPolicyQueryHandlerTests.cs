using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.GetSnsSubscriptionFilterPolicy;
using Foundation.Application.Sns;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetSnsSubscriptionFilterPolicy;

public class GetSnsSubscriptionFilterPolicyQueryHandlerTests
{
    private readonly ISnsClient _client = Substitute.For<ISnsClient>();

    private GetSnsSubscriptionFilterPolicyQueryHandler CreateSut()
        => new(_client, NullLogger<GetSnsSubscriptionFilterPolicyQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsFilterPolicy()
    {
        // Arrange
        _client
            .GetSubscriptionFilterPolicyAsync(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok("{\"store\":[\"example_corp\"]}")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSnsSubscriptionFilterPolicyQuery("arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FilterPolicy.Should().Be("{\"store\":[\"example_corp\"]}");
    }

    [Fact]
    public async Task Handle_WhenNoPolicySet_ReturnsEmptyString()
    {
        // Arrange
        _client
            .GetSubscriptionFilterPolicyAsync(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(string.Empty)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSnsSubscriptionFilterPolicyQuery("arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FilterPolicy.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetSubscriptionFilterPolicyAsync(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetSnsSubscriptionFilterPolicyQuery("arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
