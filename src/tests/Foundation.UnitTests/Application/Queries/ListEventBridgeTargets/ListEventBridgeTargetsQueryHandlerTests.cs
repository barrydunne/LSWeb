using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.EventBridge;
using Foundation.Application.Queries.ListEventBridgeTargets;
using Foundation.Domain.EventBridge;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListEventBridgeTargets;

public class ListEventBridgeTargetsQueryHandlerTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();

    private ListEventBridgeTargetsQueryHandler CreateSut()
        => new(_client, NullLogger<ListEventBridgeTargetsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsTargetsAndForwardsRuleName()
    {
        // Arrange
        IReadOnlyList<EventBridgeTarget> targets =
        [
            new("target-1", "arn:aws:lambda:eu-west-1:000000000000:function:orders-handler"),
        ];
        string? capturedRule = null;
        _client
            .ListTargetsByRuleAsync(
                Arg.Do<string>(rule => capturedRule = rule), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(targets)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListEventBridgeTargetsQuery("orders-rule"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Targets.Should().ContainSingle(_ => _.Id == "target-1");
        capturedRule.Should().Be("orders-rule");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListTargetsByRuleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<EventBridgeTarget>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListEventBridgeTargetsQuery("orders-rule"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
