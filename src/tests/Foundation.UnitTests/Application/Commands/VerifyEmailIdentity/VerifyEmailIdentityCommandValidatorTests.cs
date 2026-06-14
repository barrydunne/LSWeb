using Foundation.Application.Commands.VerifyEmailIdentity;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.VerifyEmailIdentity;

public class VerifyEmailIdentityCommandValidatorTests
{
    private readonly VerifyEmailIdentityCommandValidator _sut =
        new(NullLogger<VerifyEmailIdentityCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new VerifyEmailIdentityCommand("sender@example.com"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenEmpty_ReturnsErrorForEmailAddress()
    {
        var result = await _sut.ValidateAsync(
            new VerifyEmailIdentityCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(VerifyEmailIdentityCommand.EmailAddress));
    }

    [Fact]
    public async Task ValidateAsync_WhenMissingAtSign_ReturnsErrorForEmailAddress()
    {
        var result = await _sut.ValidateAsync(
            new VerifyEmailIdentityCommand("not-an-email"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(VerifyEmailIdentityCommand.EmailAddress));
    }
}
