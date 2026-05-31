using Foundation.Application.Commands.UntagRole;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UntagRole;

public class UntagRoleCommandValidatorTests
{
    private readonly UntagRoleCommandValidator _sut =
        new(NullLogger<UntagRoleCommandValidator>.Instance);

    private static UntagRoleCommand Valid(
        string roleName = "svc-role", IReadOnlyList<string>? tagKeys = null)
        => new(roleName, tagKeys ?? ["team"]);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenRoleNameEmpty_ReturnsErrorForRoleName()
    {
        var result = await _sut.ValidateAsync(
            Valid(roleName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UntagRoleCommand.RoleName));
    }

    [Fact]
    public async Task ValidateAsync_WhenTagKeysEmpty_ReturnsErrorForTagKeys()
    {
        var result = await _sut.ValidateAsync(
            Valid(tagKeys: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UntagRoleCommand.TagKeys));
    }

    [Fact]
    public async Task ValidateAsync_WhenTagKeyEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(tagKeys: [string.Empty]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.ErrorMessage == "Tag key must not be empty.");
    }
}
