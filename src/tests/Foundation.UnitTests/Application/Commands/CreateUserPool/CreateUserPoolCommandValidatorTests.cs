using Foundation.Application.Commands.CreateUserPool;
using Foundation.Domain.Cognito;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateUserPool;

public class CreateUserPoolCommandValidatorTests
{
    private readonly CreateUserPoolCommandValidator _sut =
        new(NullLogger<CreateUserPoolCommandValidator>.Instance);

    private static CreateUserPoolCommand Valid(
        string name = "customers",
        string? mfaConfiguration = "OFF",
        IReadOnlyList<string>? usernameAttributes = null,
        IReadOnlyList<string>? autoVerifiedAttributes = null,
        PasswordPolicy? passwordPolicy = null)
        => new(
            name,
            mfaConfiguration,
            usernameAttributes ?? ["email"],
            autoVerifiedAttributes ?? ["email"],
            passwordPolicy);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenPasswordPolicyValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(passwordPolicy: new PasswordPolicy(8, true, true, true, true)),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenPasswordPolicyMinimumLengthTooSmall_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(passwordPolicy: new PasswordPolicy(3, false, false, false, false)),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.ErrorMessage.Contains("Password minimum length"));
    }

    [Fact]
    public async Task ValidateAsync_WhenPasswordPolicyMinimumLengthTooLarge_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(passwordPolicy: new PasswordPolicy(200, false, false, false, false)),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.ErrorMessage.Contains("Password minimum length"));
    }

    [Fact]
    public async Task ValidateAsync_WhenMfaConfigurationNull_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(mfaConfiguration: null), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateUserPoolCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: new string('a', 129)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateUserPoolCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: "bad/name"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateUserPoolCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_WhenMfaConfigurationInvalid_ReturnsErrorForMfaConfiguration()
    {
        var result = await _sut.ValidateAsync(
            Valid(mfaConfiguration: "MAYBE"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateUserPoolCommand.MfaConfiguration));
    }

    [Fact]
    public async Task ValidateAsync_WhenUsernameAttributeInvalid_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(usernameAttributes: ["nickname"]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.ErrorMessage.Contains("Username attributes"));
    }

    [Fact]
    public async Task ValidateAsync_WhenAutoVerifiedAttributeInvalid_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(autoVerifiedAttributes: ["nickname"]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.ErrorMessage.Contains("Auto-verified attributes"));
    }
}
