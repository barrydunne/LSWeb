using Foundation.Application.Commands.PutSecretValue;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutSecretValue;

public class PutSecretValueCommandValidatorTests
{
    private readonly PutSecretValueCommandValidator _sut =
        new(NullLogger<PutSecretValueCommandValidator>.Instance);

    private static PutSecretValueCommand Valid(
        string secretId = "db-password",
        string secretString = "new-value")
        => new(secretId, secretString);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenSecretIdEmpty_ReturnsErrorForSecretId()
    {
        var result = await _sut.ValidateAsync(
            Valid(secretId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutSecretValueCommand.SecretId));
    }

    [Fact]
    public async Task ValidateAsync_WhenSecretStringEmpty_ReturnsErrorForSecretString()
    {
        var result = await _sut.ValidateAsync(
            Valid(secretString: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutSecretValueCommand.SecretString));
    }

    [Fact]
    public async Task ValidateAsync_WhenSecretStringTooLong_ReturnsErrorForSecretString()
    {
        var result = await _sut.ValidateAsync(
            Valid(secretString: new string('a', 65537)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutSecretValueCommand.SecretString));
    }
}
