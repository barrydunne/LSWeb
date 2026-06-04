using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.EventBridge;
using Foundation.Domain.EventBridge;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class EventBridgeResourceSourceTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();

    private EventBridgeResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsEventBridge()
        => CreateSut().ServiceKey.Should().Be("eventbridge");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsRulesToSearchEntries()
    {
        // Arrange
        IReadOnlyList<EventBridgeRule> rules =
        [
            new(
                "orders rule",
                "arn:aws:events:eu-west-1:000000000000:rule/orders rule",
                "default",
                "ENABLED",
                null,
                null),
        ];
        _client
            .ListRulesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(rules)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("eventbridge");
        entry.ResourceId.Should().Be("orders rule");
        entry.DisplayName.Should().Be("orders rule");
        entry.Route.Should().Be("/services/eventbridge/orders%20rule");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListRulesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<EventBridgeRule>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
