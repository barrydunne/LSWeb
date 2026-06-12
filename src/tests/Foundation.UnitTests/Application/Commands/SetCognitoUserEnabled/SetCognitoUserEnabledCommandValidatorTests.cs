using Foundation.Application.Commands.SetCognitoUserEnabled;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetCognitoUserEnabled;

public class SetCognitoUserEnabledCommandValidatorTests
{
    private readonly SetCognitoUserEnabledCommandValidator _sut =
        new(NullLogger<SetCognitoUserEnabledCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new SetCognitoUserEnabledCommand("eu-west-1_abc123", "alice", true), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenUserPoolIdEmpty_ReturnsErrorForUserPoolId()
    {
        var result = await _sut.ValidateAsync(
            new SetCognitoUserEnabledCommand(string.Empty, "alice", true), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetCognitoUserEnabledCommand.UserPoolId));
    }

    [Fact]
    public async Task ValidateAsync_WhenUsernameEmpty_ReturnsErrorForUsername()
    {
        var result = await _sut.ValidateAsync(
            new SetCognitoUserEnabledCommand("eu-west-1_abc123", string.Empty, false), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetCognitoUserEnabledCommand.Username));
    }
}
