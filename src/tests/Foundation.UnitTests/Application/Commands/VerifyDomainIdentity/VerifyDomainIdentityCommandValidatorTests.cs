using Foundation.Application.Commands.VerifyDomainIdentity;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.VerifyDomainIdentity;

public class VerifyDomainIdentityCommandValidatorTests
{
    private readonly VerifyDomainIdentityCommandValidator _sut =
        new(NullLogger<VerifyDomainIdentityCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new VerifyDomainIdentityCommand("example.com"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenEmpty_ReturnsErrorForDomain()
    {
        var result = await _sut.ValidateAsync(
            new VerifyDomainIdentityCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(VerifyDomainIdentityCommand.Domain));
    }

    [Theory]
    [InlineData("nodot")]
    [InlineData("user@example.com")]
    public async Task ValidateAsync_WhenNotADomain_ReturnsErrorForDomain(string domain)
    {
        var result = await _sut.ValidateAsync(
            new VerifyDomainIdentityCommand(domain), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(VerifyDomainIdentityCommand.Domain));
    }
}
