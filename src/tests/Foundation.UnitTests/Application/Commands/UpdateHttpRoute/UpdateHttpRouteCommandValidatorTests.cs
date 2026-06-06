using Foundation.Application.Commands.UpdateHttpRoute;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateHttpRoute;

public class UpdateHttpRouteCommandValidatorTests
{
    private readonly UpdateHttpRouteCommandValidator _sut =
        new(NullLogger<UpdateHttpRouteCommandValidator>.Instance);

    private static UpdateHttpRouteCommand Valid(
        string apiId = "abc123",
        string routeId = "route1",
        string routeKey = "GET /items")
        => new(apiId, routeId, routeKey, "integrations/int1", "NONE", null, []);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateHttpRouteCommand.ApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenRouteIdEmpty_ReturnsErrorForRouteId()
    {
        var result = await _sut.ValidateAsync(
            Valid(routeId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateHttpRouteCommand.RouteId));
    }

    [Fact]
    public async Task ValidateAsync_WhenRouteKeyMalformed_ReturnsErrorForRouteKey()
    {
        var result = await _sut.ValidateAsync(
            Valid(routeKey: "items"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateHttpRouteCommand.RouteKey));
    }
}
