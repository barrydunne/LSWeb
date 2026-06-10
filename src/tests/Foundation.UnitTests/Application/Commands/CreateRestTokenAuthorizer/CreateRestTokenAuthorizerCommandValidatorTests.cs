using Foundation.Application.Commands.CreateRestTokenAuthorizer;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateRestTokenAuthorizer;

public class CreateRestTokenAuthorizerCommandValidatorTests
{
    private readonly CreateRestTokenAuthorizerCommandValidator _sut =
        new(NullLogger<CreateRestTokenAuthorizerCommandValidator>.Instance);

    private static CreateRestTokenAuthorizerCommand Valid(
        string restApiId = "api-1",
        string name = "jwt-authorizer",
        string issuer = "https://issuer.example.com",
        string audience = "api://default",
        string identitySource = "method.request.header.Authorization",
        string authorizerUri = "arn:aws:apigateway:eu-west-1:lambda:path/invocations")
        => new(restApiId, name, issuer, audience, identitySource, authorizerUri);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestTokenAuthorizerCommand.RestApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestTokenAuthorizerCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: new string('a', 257)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestTokenAuthorizerCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenIssuerEmpty_ReturnsErrorForIssuer()
    {
        var result = await _sut.ValidateAsync(
            Valid(issuer: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestTokenAuthorizerCommand.Issuer));
    }

    [Fact]
    public async Task ValidateAsync_WhenIssuerNotHttps_ReturnsErrorForIssuer()
    {
        var result = await _sut.ValidateAsync(
            Valid(issuer: "http://issuer.example.com"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestTokenAuthorizerCommand.Issuer));
    }

    [Fact]
    public async Task ValidateAsync_WhenIssuerNotAbsoluteUri_ReturnsErrorForIssuer()
    {
        var result = await _sut.ValidateAsync(
            Valid(issuer: "not a uri"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestTokenAuthorizerCommand.Issuer));
    }

    [Fact]
    public async Task ValidateAsync_WhenAudienceEmpty_ReturnsErrorForAudience()
    {
        var result = await _sut.ValidateAsync(
            Valid(audience: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestTokenAuthorizerCommand.Audience));
    }

    [Fact]
    public async Task ValidateAsync_WhenIdentitySourceEmpty_ReturnsErrorForIdentitySource()
    {
        var result = await _sut.ValidateAsync(
            Valid(identitySource: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestTokenAuthorizerCommand.IdentitySource));
    }

    [Fact]
    public async Task ValidateAsync_WhenIdentitySourceNotRequestValue_ReturnsErrorForIdentitySource()
    {
        var result = await _sut.ValidateAsync(
            Valid(identitySource: "header.Authorization"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestTokenAuthorizerCommand.IdentitySource));
    }

    [Fact]
    public async Task ValidateAsync_WhenIdentitySourceIsPrefixOnly_ReturnsErrorForIdentitySource()
    {
        var result = await _sut.ValidateAsync(
            Valid(identitySource: "method.request."), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestTokenAuthorizerCommand.IdentitySource));
    }

    [Fact]
    public async Task ValidateAsync_WhenAuthorizerUriEmpty_ReturnsErrorForAuthorizerUri()
    {
        var result = await _sut.ValidateAsync(
            Valid(authorizerUri: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateRestTokenAuthorizerCommand.AuthorizerUri));
    }
}
