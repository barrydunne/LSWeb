using Foundation.Application.Commands.EnableDomainDkim;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.EnableDomainDkim;

public class EnableDomainDkimCommandValidatorTests
{
    private readonly EnableDomainDkimCommandValidator _sut =
        new(NullLogger<EnableDomainDkimCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new EnableDomainDkimCommand("example.com"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenEmpty_ReturnsErrorForDomain()
    {
        var result = await _sut.ValidateAsync(
            new EnableDomainDkimCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(EnableDomainDkimCommand.Domain));
    }
}
