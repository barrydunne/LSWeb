using Foundation.Application.Commands.PutEventBridgeEvent;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutEventBridgeEvent;

public class PutEventBridgeEventCommandValidatorTests
{
    private readonly PutEventBridgeEventCommandValidator _sut =
        new(NullLogger<PutEventBridgeEventCommandValidator>.Instance);

    private static PutEventBridgeEventCommand Command(
        string source = "orders.service",
        string detailType = "OrderPlaced",
        string detail = "{\"orderId\":\"abc\"}",
        string? eventBusName = "orders-bus")
        => new(source, detailType, detail, eventBusName);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Command(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenEventBusNameNull_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Command(eventBusName: null), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenSourceEmpty_ReturnsErrorForSource()
    {
        var result = await _sut.ValidateAsync(
            Command(source: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutEventBridgeEventCommand.Source));
    }

    [Fact]
    public async Task ValidateAsync_WhenDetailTypeEmpty_ReturnsErrorForDetailType()
    {
        var result = await _sut.ValidateAsync(
            Command(detailType: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutEventBridgeEventCommand.DetailType));
    }

    [Fact]
    public async Task ValidateAsync_WhenDetailEmpty_ReturnsErrorForDetail()
    {
        var result = await _sut.ValidateAsync(
            Command(detail: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutEventBridgeEventCommand.Detail));
    }

    [Fact]
    public async Task ValidateAsync_WhenDetailNotJsonObject_ReturnsErrorForDetail()
    {
        var result = await _sut.ValidateAsync(
            Command(detail: "[1,2,3]"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutEventBridgeEventCommand.Detail));
    }

    [Fact]
    public async Task ValidateAsync_WhenDetailInvalidJson_ReturnsErrorForDetail()
    {
        var result = await _sut.ValidateAsync(
            Command(detail: "{not json"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutEventBridgeEventCommand.Detail));
    }
}
