using Foundation.Application.Commands.DeleteCognitoUser;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteCognitoUser;

public class DeleteCognitoUserCommandValidatorTests
{
    private readonly DeleteCognitoUserCommandValidator _sut =
        new(NullLogger<DeleteCognitoUserCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteCognitoUserCommand("eu-west-1_abc123", "alice"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenUserPoolIdEmpty_ReturnsErrorForUserPoolId()
    {
        var result = await _sut.ValidateAsync(
            new DeleteCognitoUserCommand(string.Empty, "alice"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteCognitoUserCommand.UserPoolId));
    }

    [Fact]
    public async Task ValidateAsync_WhenUsernameEmpty_ReturnsErrorForUsername()
    {
        var result = await _sut.ValidateAsync(
            new DeleteCognitoUserCommand("eu-west-1_abc123", string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteCognitoUserCommand.Username));
    }
}
