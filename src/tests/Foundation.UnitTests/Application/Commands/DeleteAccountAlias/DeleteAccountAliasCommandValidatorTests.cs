using Foundation.Application.Commands.DeleteAccountAlias;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteAccountAlias;

public class DeleteAccountAliasCommandValidatorTests
{
    private readonly DeleteAccountAliasCommandValidator _sut =
        new(NullLogger<DeleteAccountAliasCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteAccountAliasCommand("my-account"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenEmpty_ReturnsErrorForAccountAlias()
    {
        var result = await _sut.ValidateAsync(
            new DeleteAccountAliasCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteAccountAliasCommand.AccountAlias));
    }
}
