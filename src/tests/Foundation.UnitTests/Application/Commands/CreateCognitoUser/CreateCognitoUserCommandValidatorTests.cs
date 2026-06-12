using Foundation.Application.Commands.CreateCognitoUser;
using Foundation.Domain.Cognito;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateCognitoUser;

public class CreateCognitoUserCommandValidatorTests
{
    private readonly CreateCognitoUserCommandValidator _sut =
        new(NullLogger<CreateCognitoUserCommandValidator>.Instance);

    private static CreateCognitoUserCommand Valid(
        string userPoolId = "eu-west-1_abc123",
        string username = "alice",
        IReadOnlyList<CognitoUserAttributeEntry>? attributes = null)
        => new(userPoolId, username, attributes ?? [new CognitoUserAttributeEntry("email", "alice@example.com")], null);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateCognitoUserCommand.UserPoolId));
    }

    [Fact]
    public async Task ValidateAsync_WhenUsernameEmpty_ReturnsErrorForUsername()
    {
        var result = await _sut.ValidateAsync(
            Valid(username: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateCognitoUserCommand.Username));
    }

    [Fact]
    public async Task ValidateAsync_WhenUsernameTooLong_ReturnsErrorForUsername()
    {
        var result = await _sut.ValidateAsync(
            Valid(username: new string('a', 129)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateCognitoUserCommand.Username));
    }

    [Fact]
    public async Task ValidateAsync_WhenAttributeNameEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(attributes: [new CognitoUserAttributeEntry(string.Empty, "value")]),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.ErrorMessage.Contains("attribute names"));
    }
}
