using Foundation.Application.Commands.SetCognitoUserPassword;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetCognitoUserPassword;

public class SetCognitoUserPasswordCommandValidatorTests
{
    private readonly SetCognitoUserPasswordCommandValidator _sut =
        new(NullLogger<SetCognitoUserPasswordCommandValidator>.Instance);

    private static SetCognitoUserPasswordCommand Valid(
        string userPoolId = "eu-west-1_abc123",
        string username = "alice",
        string password = "NewPass1!")
        => new(userPoolId, username, password, true);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetCognitoUserPasswordCommand.UserPoolId));
    }

    [Fact]
    public async Task ValidateAsync_WhenUsernameEmpty_ReturnsErrorForUsername()
    {
        var result = await _sut.ValidateAsync(
            Valid(username: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetCognitoUserPasswordCommand.Username));
    }

    [Fact]
    public async Task ValidateAsync_WhenPasswordEmpty_ReturnsErrorForPassword()
    {
        var result = await _sut.ValidateAsync(
            Valid(password: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetCognitoUserPasswordCommand.Password));
    }

    [Fact]
    public async Task ValidateAsync_WhenPasswordTooShort_ReturnsErrorForPassword()
    {
        var result = await _sut.ValidateAsync(
            Valid(password: "abc"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetCognitoUserPasswordCommand.Password));
    }
}
