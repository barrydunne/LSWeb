using Foundation.Application.Commands.ConfigureRestCors;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.ConfigureRestCors;

public class ConfigureRestCorsCommandValidatorTests
{
    private readonly ConfigureRestCorsCommandValidator _sut =
        new(NullLogger<ConfigureRestCorsCommandValidator>.Instance);

    private static ConfigureRestCorsCommand Valid(
        string restApiId = "api-1",
        string resourceId = "resource-1",
        IReadOnlyList<string>? allowOrigins = null,
        IReadOnlyList<string>? allowMethods = null,
        IReadOnlyList<string>? allowHeaders = null)
        => new(
            restApiId,
            resourceId,
            allowOrigins ?? ["*"],
            allowMethods ?? ["GET", "POST"],
            allowHeaders ?? ["Content-Type"]);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenValidAndHeadersEmpty_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(allowHeaders: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenRestApiIdEmpty_ReturnsErrorForRestApiId()
    {
        var result = await _sut.ValidateAsync(
            Valid(restApiId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ConfigureRestCorsCommand.RestApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenResourceIdEmpty_ReturnsErrorForResourceId()
    {
        var result = await _sut.ValidateAsync(
            Valid(resourceId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ConfigureRestCorsCommand.ResourceId));
    }

    [Fact]
    public async Task ValidateAsync_WhenAllowOriginsEmpty_ReturnsErrorForAllowOrigins()
    {
        var result = await _sut.ValidateAsync(
            Valid(allowOrigins: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ConfigureRestCorsCommand.AllowOrigins));
    }

    [Fact]
    public async Task ValidateAsync_WhenAllowOriginContainsBlank_ReturnsErrorForAllowOrigins()
    {
        var result = await _sut.ValidateAsync(
            Valid(allowOrigins: [" "]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ConfigureRestCorsCommand.AllowOrigins));
    }

    [Fact]
    public async Task ValidateAsync_WhenAllowMethodsEmpty_ReturnsErrorForAllowMethods()
    {
        var result = await _sut.ValidateAsync(
            Valid(allowMethods: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ConfigureRestCorsCommand.AllowMethods));
    }

    [Fact]
    public async Task ValidateAsync_WhenAllowMethodNotHttpVerb_ReturnsErrorForAllowMethods()
    {
        var result = await _sut.ValidateAsync(
            Valid(allowMethods: ["FETCH"]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(ConfigureRestCorsCommand.AllowMethods));
    }
}
