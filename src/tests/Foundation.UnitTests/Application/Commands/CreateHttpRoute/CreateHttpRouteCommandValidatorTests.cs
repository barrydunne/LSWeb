using Foundation.Application.Commands.CreateHttpRoute;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateHttpRoute;

public class CreateHttpRouteCommandValidatorTests
{
    private readonly CreateHttpRouteCommandValidator _sut =
        new(NullLogger<CreateHttpRouteCommandValidator>.Instance);

    private static CreateHttpRouteCommand Valid(
        string apiId = "abc123",
        string routeKey = "GET /items")
        => new(apiId, routeKey, "integrations/int1", "NONE", null, []);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenDefaultRouteKey_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(routeKey: "$default"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenApiIdEmpty_ReturnsErrorForApiId()
    {
        var result = await _sut.ValidateAsync(
            Valid(apiId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpRouteCommand.ApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenRouteKeyEmpty_ReturnsErrorForRouteKey()
    {
        var result = await _sut.ValidateAsync(
            Valid(routeKey: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpRouteCommand.RouteKey));
    }

    [Fact]
    public async Task ValidateAsync_WhenRouteKeyTooLong_ReturnsErrorForRouteKey()
    {
        var result = await _sut.ValidateAsync(
            Valid(routeKey: "GET /" + new string('a', 256)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpRouteCommand.RouteKey));
    }

    [Fact]
    public async Task ValidateAsync_WhenRouteKeyMalformed_ReturnsErrorForRouteKey()
    {
        var result = await _sut.ValidateAsync(
            Valid(routeKey: "items"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpRouteCommand.RouteKey));
    }
}
