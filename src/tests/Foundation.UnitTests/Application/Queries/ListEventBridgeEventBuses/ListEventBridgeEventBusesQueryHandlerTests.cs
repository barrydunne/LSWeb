using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.EventBridge;
using Foundation.Application.Queries.ListEventBridgeEventBuses;
using Foundation.Domain.EventBridge;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListEventBridgeEventBuses;

public class ListEventBridgeEventBusesQueryHandlerTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();

    private static Result<T> Ok<T>(T value)
        => value;

    private ListEventBridgeEventBusesQueryHandler CreateSut()
        => new(_client, NullLogger<ListEventBridgeEventBusesQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsBuses()
    {
        // Arrange
        IReadOnlyList<EventBridgeEventBus> buses =
        [
            new("default", "arn:aws:events:eu-west-1:000000000000:event-bus/default"),
        ];
        _client
            .ListEventBusesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(buses)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListEventBridgeEventBusesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var bus = result.Value.Buses.Should().ContainSingle().Subject;
        bus.Name.Should().Be("default");
        bus.Arn.Should().Be("arn:aws:events:eu-west-1:000000000000:event-bus/default");
    }

    [Fact]
    public async Task Handle_WhenClientFails_ReturnsError()
    {
        // Arrange
        _client
            .ListEventBusesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<EventBridgeEventBus>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListEventBridgeEventBusesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
