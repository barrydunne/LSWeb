using Foundation.Application.Commands.CreateHttpApi;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateHttpApi;

public class CreateHttpApiCommandValidatorTests
{
    private readonly CreateHttpApiCommandValidator _sut =
        new(NullLogger<CreateHttpApiCommandValidator>.Instance);

    private static CreateHttpApiCommand Valid(
        string name = "orders",
        string protocolType = "HTTP",
        string? routeSelectionExpression = null)
        => new(name, protocolType, "desc", "1.0", routeSelectionExpression);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenWebSocketWithRouteSelection_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(protocolType: "WEBSOCKET", routeSelectionExpression: "$request.body.action"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpApiCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: new string('a', 129)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpApiCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenProtocolTypeEmpty_ReturnsErrorForProtocolType()
    {
        var result = await _sut.ValidateAsync(
            Valid(protocolType: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpApiCommand.ProtocolType));
    }

    [Fact]
    public async Task ValidateAsync_WhenProtocolTypeInvalid_ReturnsErrorForProtocolType()
    {
        var result = await _sut.ValidateAsync(
            Valid(protocolType: "GRPC"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpApiCommand.ProtocolType));
    }

    [Fact]
    public async Task ValidateAsync_WhenWebSocketWithoutRouteSelection_ReturnsErrorForRouteSelectionExpression()
    {
        var result = await _sut.ValidateAsync(
            Valid(protocolType: "WEBSOCKET", routeSelectionExpression: null),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpApiCommand.RouteSelectionExpression));
    }
}
