using Foundation.Domain.Lambda;
using Foundation.Infrastructure.Lambda;

namespace Foundation.UnitTests.Infrastructure.Lambda;

public class InMemoryTestEventStoreTests
{
    private readonly InMemoryTestEventStore _sut = new();

    [Fact]
    public async Task GetEventsAsync_WhenNoEventsSaved_ReturnsEmptyList()
    {
        // Act
        var result = await _sut.GetEventsAsync("orders", TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveEventAsync_ThenGetEventsAsync_ReturnsSavedEvent()
    {
        // Arrange
        var testEvent = new LambdaTestEvent("first", "{\"a\":1}");

        // Act
        var saveResult = await _sut.SaveEventAsync("orders", testEvent, TestContext.Current.CancellationToken);
        var getResult = await _sut.GetEventsAsync("orders", TestContext.Current.CancellationToken);

        // Assert
        saveResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(testEvent);
    }

    [Fact]
    public async Task SaveEventAsync_WhenSameName_ReplacesExistingEvent()
    {
        // Arrange
        await _sut.SaveEventAsync("orders", new LambdaTestEvent("dup", "{\"v\":1}"), TestContext.Current.CancellationToken);

        // Act
        await _sut.SaveEventAsync("orders", new LambdaTestEvent("dup", "{\"v\":2}"), TestContext.Current.CancellationToken);
        var getResult = await _sut.GetEventsAsync("orders", TestContext.Current.CancellationToken);

        // Assert
        getResult.Value.Should().ContainSingle()
            .Which.Payload.Should().Be("{\"v\":2}");
    }

    [Fact]
    public async Task DeleteEventAsync_WhenEventExists_RemovesIt()
    {
        // Arrange
        await _sut.SaveEventAsync("orders", new LambdaTestEvent("gone", "{}"), TestContext.Current.CancellationToken);

        // Act
        var deleteResult = await _sut.DeleteEventAsync("orders", "gone", TestContext.Current.CancellationToken);
        var getResult = await _sut.GetEventsAsync("orders", TestContext.Current.CancellationToken);

        // Assert
        deleteResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteEventAsync_WhenFunctionUnknown_SucceedsSilently()
    {
        // Act
        var result = await _sut.DeleteEventAsync("missing", "nope", TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteEventAsync_WhenEventNameUnknown_SucceedsSilently()
    {
        // Arrange
        await _sut.SaveEventAsync("orders", new LambdaTestEvent("keep", "{}"), TestContext.Current.CancellationToken);

        // Act
        var deleteResult = await _sut.DeleteEventAsync("orders", "other", TestContext.Current.CancellationToken);
        var getResult = await _sut.GetEventsAsync("orders", TestContext.Current.CancellationToken);

        // Assert
        deleteResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().ContainSingle().Which.Name.Should().Be("keep");
    }

    [Fact]
    public async Task SaveEventAsync_KeepsEventsForDifferentFunctionsIsolated()
    {
        // Arrange
        await _sut.SaveEventAsync("orders", new LambdaTestEvent("a", "{}"), TestContext.Current.CancellationToken);
        await _sut.SaveEventAsync("billing", new LambdaTestEvent("b", "{}"), TestContext.Current.CancellationToken);

        // Act
        var orders = await _sut.GetEventsAsync("orders", TestContext.Current.CancellationToken);
        var billing = await _sut.GetEventsAsync("billing", TestContext.Current.CancellationToken);

        // Assert
        orders.Value.Should().ContainSingle().Which.Name.Should().Be("a");
        billing.Value.Should().ContainSingle().Which.Name.Should().Be("b");
    }
}
