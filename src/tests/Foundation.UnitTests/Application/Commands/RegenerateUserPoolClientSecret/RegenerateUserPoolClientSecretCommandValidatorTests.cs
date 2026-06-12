using Foundation.Application.Commands.RegenerateUserPoolClientSecret;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RegenerateUserPoolClientSecret;

public class RegenerateUserPoolClientSecretCommandValidatorTests
{
    private readonly RegenerateUserPoolClientSecretCommandValidator _sut =
        new(NullLogger<RegenerateUserPoolClientSecretCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new RegenerateUserPoolClientSecretCommand("eu-west-1_abc123", "client-1"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenUserPoolIdEmpty_ReturnsErrorForUserPoolId()
    {
        var result = await _sut.ValidateAsync(
            new RegenerateUserPoolClientSecretCommand(string.Empty, "client-1"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RegenerateUserPoolClientSecretCommand.UserPoolId));
    }

    [Fact]
    public async Task ValidateAsync_WhenClientIdEmpty_ReturnsErrorForClientId()
    {
        var result = await _sut.ValidateAsync(
            new RegenerateUserPoolClientSecretCommand("eu-west-1_abc123", string.Empty),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RegenerateUserPoolClientSecretCommand.ClientId));
    }
}
