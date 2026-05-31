using Foundation.Application.Commands.TagRole;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.TagRole;

public class TagRoleCommandValidatorTests
{
    private readonly TagRoleCommandValidator _sut =
        new(NullLogger<TagRoleCommandValidator>.Instance);

    private static TagRoleCommand Valid(
        string roleName = "svc-role", IReadOnlyList<IamTag>? tags = null)
        => new(roleName, tags ?? [new IamTag("team", "platform")]);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TagRoleCommand.RoleName));
    }

    [Fact]
    public async Task ValidateAsync_WhenTagsEmpty_ReturnsErrorForTags()
    {
        var result = await _sut.ValidateAsync(
            Valid(tags: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TagRoleCommand.Tags));
    }

    [Fact]
    public async Task ValidateAsync_WhenTagKeyEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(tags: [new IamTag(string.Empty, "platform")]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.ErrorMessage == "Tag key must not be empty.");
    }
}
