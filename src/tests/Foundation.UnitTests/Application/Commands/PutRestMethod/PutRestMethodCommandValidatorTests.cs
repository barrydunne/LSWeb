using Foundation.Application.Commands.PutRestMethod;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutRestMethod;

public class PutRestMethodCommandValidatorTests
{
    private readonly PutRestMethodCommandValidator _sut =
        new(NullLogger<PutRestMethodCommandValidator>.Instance);

    private static PutRestMethodCommand Valid(
        string restApiId = "api-1",
        string resourceId = "res-2",
        string httpMethod = "GET",
        string authorizationType = "NONE",
        IReadOnlyList<string>? authorizationScopes = null)
        => new(restApiId, resourceId, httpMethod, authorizationType, null, false, authorizationScopes ?? []);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenRestApiIdEmpty_ReturnsErrorForRestApiId()
    {
        var result = await _sut.ValidateAsync(
            Valid(restApiId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRestMethodCommand.RestApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenResourceIdEmpty_ReturnsErrorForResourceId()
    {
        var result = await _sut.ValidateAsync(
            Valid(resourceId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRestMethodCommand.ResourceId));
    }

    [Fact]
    public async Task ValidateAsync_WhenHttpMethodEmpty_ReturnsErrorForHttpMethod()
    {
        var result = await _sut.ValidateAsync(
            Valid(httpMethod: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRestMethodCommand.HttpMethod));
    }

    [Fact]
    public async Task ValidateAsync_WhenHttpMethodInvalid_ReturnsErrorForHttpMethod()
    {
        var result = await _sut.ValidateAsync(
            Valid(httpMethod: "FETCH"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRestMethodCommand.HttpMethod));
    }

    [Fact]
    public async Task ValidateAsync_WhenAuthorizationTypeEmpty_ReturnsErrorForAuthorizationType()
    {
        var result = await _sut.ValidateAsync(
            Valid(authorizationType: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRestMethodCommand.AuthorizationType));
    }

    [Fact]
    public async Task ValidateAsync_WhenAuthorizationTypeInvalid_ReturnsErrorForAuthorizationType()
    {
        var result = await _sut.ValidateAsync(
            Valid(authorizationType: "MAGIC"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRestMethodCommand.AuthorizationType));
    }

    [Fact]
    public async Task ValidateAsync_WhenAuthorizationScopesNull_ReturnsErrorForAuthorizationScopes()
    {
        var command = new PutRestMethodCommand("api-1", "res-2", "GET", "NONE", null, false, null!);
        var result = await _sut.ValidateAsync(command, TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRestMethodCommand.AuthorizationScopes));
    }
}
