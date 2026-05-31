using Foundation.Application.Commands.CreateAccountAlias;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateAccountAlias;

public class CreateAccountAliasCommandValidatorTests
{
    private readonly CreateAccountAliasCommandValidator _sut =
        new(NullLogger<CreateAccountAliasCommandValidator>.Instance);

    private static CreateAccountAliasCommand Build(string alias)
        => new(alias);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Build("my-account-01"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenEmpty_ReturnsErrorForAccountAlias()
    {
        var result = await _sut.ValidateAsync(Build(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateAccountAliasCommand.AccountAlias));
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("My-Account")]
    [InlineData("-account")]
    [InlineData("account-")]
    [InlineData("account_name")]
    public async Task ValidateAsync_WhenInvalidFormat_ReturnsErrorForAccountAlias(string alias)
    {
        var result = await _sut.ValidateAsync(Build(alias), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateAccountAliasCommand.AccountAlias));
    }
}
