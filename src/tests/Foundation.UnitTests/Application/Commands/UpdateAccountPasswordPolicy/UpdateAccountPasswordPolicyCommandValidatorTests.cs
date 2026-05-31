using Foundation.Application.Commands.UpdateAccountPasswordPolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateAccountPasswordPolicy;

public class UpdateAccountPasswordPolicyCommandValidatorTests
{
    private readonly UpdateAccountPasswordPolicyCommandValidator _sut =
        new(NullLogger<UpdateAccountPasswordPolicyCommandValidator>.Instance);

    private static UpdateAccountPasswordPolicyCommand Valid(
        int minimumPasswordLength = 14,
        int? maxPasswordAge = 90,
        int? passwordReusePrevention = 5)
        => new(
            MinimumPasswordLength: minimumPasswordLength,
            RequireSymbols: true,
            RequireNumbers: true,
            RequireUppercaseCharacters: true,
            RequireLowercaseCharacters: true,
            AllowUsersToChangePassword: true,
            MaxPasswordAge: maxPasswordAge,
            PasswordReusePrevention: passwordReusePrevention,
            HardExpiry: false);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNullableRangesNull_IsValid()
    {
        var result = await _sut.ValidateAsync(
            Valid(maxPasswordAge: null, passwordReusePrevention: null), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(129)]
    public async Task ValidateAsync_WhenMinimumPasswordLengthOutOfRange_ReturnsError(int length)
    {
        var result = await _sut.ValidateAsync(
            Valid(minimumPasswordLength: length), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateAccountPasswordPolicyCommand.MinimumPasswordLength));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1096)]
    public async Task ValidateAsync_WhenMaxPasswordAgeOutOfRange_ReturnsError(int age)
    {
        var result = await _sut.ValidateAsync(
            Valid(maxPasswordAge: age), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateAccountPasswordPolicyCommand.MaxPasswordAge));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    public async Task ValidateAsync_WhenPasswordReusePreventionOutOfRange_ReturnsError(int reuse)
    {
        var result = await _sut.ValidateAsync(
            Valid(passwordReusePrevention: reuse), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateAccountPasswordPolicyCommand.PasswordReusePrevention));
    }
}
