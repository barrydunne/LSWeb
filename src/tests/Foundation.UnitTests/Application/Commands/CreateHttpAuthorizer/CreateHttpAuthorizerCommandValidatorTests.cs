using Foundation.Application.Commands.CreateHttpAuthorizer;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateHttpAuthorizer;

public class CreateHttpAuthorizerCommandValidatorTests
{
    private readonly CreateHttpAuthorizerCommandValidator _sut =
        new(NullLogger<CreateHttpAuthorizerCommandValidator>.Instance);

    private static CreateHttpAuthorizerCommand Valid(
        string apiId = "abc123",
        string name = "jwt-authorizer",
        string authorizerType = "JWT",
        string? jwtIssuer = "https://example.com/issuer",
        IReadOnlyList<string>? jwtAudience = null)
        => new(
            apiId,
            name,
            authorizerType,
            ["$request.header.Authorization"],
            jwtIssuer,
            jwtAudience ?? ["client1"]);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpAuthorizerCommand.ApiId));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpAuthorizerCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: new string('a', 129)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpAuthorizerCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenAuthorizerTypeNotJwt_ReturnsErrorForAuthorizerType()
    {
        var result = await _sut.ValidateAsync(
            Valid(authorizerType: "REQUEST"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpAuthorizerCommand.AuthorizerType));
    }

    [Fact]
    public async Task ValidateAsync_WhenJwtIssuerEmpty_ReturnsErrorForJwtIssuer()
    {
        var result = await _sut.ValidateAsync(
            Valid(jwtIssuer: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpAuthorizerCommand.JwtIssuer));
    }

    [Fact]
    public async Task ValidateAsync_WhenJwtIssuerNotAbsoluteUri_ReturnsErrorForJwtIssuer()
    {
        var result = await _sut.ValidateAsync(
            Valid(jwtIssuer: "not a uri"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpAuthorizerCommand.JwtIssuer));
    }

    [Fact]
    public async Task ValidateAsync_WhenJwtAudienceEmpty_ReturnsErrorForJwtAudience()
    {
        var result = await _sut.ValidateAsync(
            Valid(jwtAudience: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateHttpAuthorizerCommand.JwtAudience));
    }
}
