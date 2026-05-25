using Foundation.Application.Commands.CreateSecret;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateSecret;

public class CreateSecretCommandValidatorTests
{
    private readonly CreateSecretCommandValidator _sut =
        new(NullLogger<CreateSecretCommandValidator>.Instance);

    private static CreateSecretCommand Valid(
        string name = "db-password",
        string? description = null,
        string secretString = "s3cr3t")
        => new(name, description, secretString);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenDescriptionProvided_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(description: "primary database password"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateSecretCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: new string('a', 513)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateSecretCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: "bad name!"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateSecretCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenDescriptionTooLong_ReturnsErrorForDescription()
    {
        var result = await _sut.ValidateAsync(
            Valid(description: new string('a', 2049)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateSecretCommand.Description));
    }

    [Fact]
    public async Task ValidateAsync_WhenSecretStringEmpty_ReturnsErrorForSecretString()
    {
        var result = await _sut.ValidateAsync(
            Valid(secretString: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateSecretCommand.SecretString));
    }

    [Fact]
    public async Task ValidateAsync_WhenSecretStringTooLong_ReturnsErrorForSecretString()
    {
        var result = await _sut.ValidateAsync(
            Valid(secretString: new string('a', 65537)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateSecretCommand.SecretString));
    }
}
