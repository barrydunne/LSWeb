using Foundation.Application.Commands.UpdateHttpApi;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateHttpApi;

public class UpdateHttpApiCommandValidatorTests
{
    private readonly UpdateHttpApiCommandValidator _sut =
        new(NullLogger<UpdateHttpApiCommandValidator>.Instance);

    private static UpdateHttpApiCommand Valid(
        string apiId = "abc123",
        string name = "orders",
        string protocolType = "HTTP",
        string? routeSelectionExpression = null)
        => new(apiId, name, protocolType, "desc", "1.0", routeSelectionExpression);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenApiIdEmpty_ReturnsErrorForApiId()
    {
        var result = await _sut.ValidateAsync(
            Valid(apiId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateHttpApiCommand.ApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateHttpApiCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: new string('a', 129)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateHttpApiCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenProtocolTypeInvalid_ReturnsErrorForProtocolType()
    {
        var result = await _sut.ValidateAsync(
            Valid(protocolType: "GRPC"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateHttpApiCommand.ProtocolType));
    }

    [Fact]
    public async Task ValidateAsync_WhenWebSocketWithoutRouteSelection_ReturnsErrorForRouteSelectionExpression()
    {
        var result = await _sut.ValidateAsync(
            Valid(protocolType: "WEBSOCKET", routeSelectionExpression: null),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateHttpApiCommand.RouteSelectionExpression));
    }
}
