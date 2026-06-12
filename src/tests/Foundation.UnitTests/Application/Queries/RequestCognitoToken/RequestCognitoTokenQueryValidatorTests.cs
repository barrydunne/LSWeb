using Foundation.Application.Queries.RequestCognitoToken;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.RequestCognitoToken;

public class RequestCognitoTokenQueryValidatorTests
{
    private readonly RequestCognitoTokenQueryValidator _sut =
        new(NullLogger<RequestCognitoTokenQueryValidator>.Instance);

    private static RequestCognitoTokenQuery Valid(
        string userPoolId = "eu-west-1_abc123",
        string clientId = "client-1",
        string username = "alice",
        string password = "Passw0rd!")
        => new(userPoolId, clientId, username, password);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenUserPoolIdEmpty_ReturnsErrorForUserPoolId()
    {
        var result = await _sut.ValidateAsync(
            Valid(userPoolId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RequestCognitoTokenQuery.UserPoolId));
    }

    [Fact]
    public async Task ValidateAsync_WhenClientIdEmpty_ReturnsErrorForClientId()
    {
        var result = await _sut.ValidateAsync(
            Valid(clientId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RequestCognitoTokenQuery.ClientId));
    }

    [Fact]
    public async Task ValidateAsync_WhenUsernameEmpty_ReturnsErrorForUsername()
    {
        var result = await _sut.ValidateAsync(
            Valid(username: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RequestCognitoTokenQuery.Username));
    }

    [Fact]
    public async Task ValidateAsync_WhenPasswordEmpty_ReturnsErrorForPassword()
    {
        var result = await _sut.ValidateAsync(
            Valid(password: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RequestCognitoTokenQuery.Password));
    }
}
