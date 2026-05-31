using Foundation.Application.Commands.TagUser;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.TagUser;

public class TagUserCommandValidatorTests
{
    private readonly TagUserCommandValidator _sut =
        new(NullLogger<TagUserCommandValidator>.Instance);

    private static TagUserCommand Valid(
        string userName = "alice", IReadOnlyList<IamTag>? tags = null)
        => new(userName, tags ?? [new IamTag("env", "dev")]);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenUserNameEmpty_ReturnsErrorForUserName()
    {
        var result = await _sut.ValidateAsync(
            Valid(userName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TagUserCommand.UserName));
    }

    [Fact]
    public async Task ValidateAsync_WhenTagsEmpty_ReturnsErrorForTags()
    {
        var result = await _sut.ValidateAsync(
            Valid(tags: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TagUserCommand.Tags));
    }

    [Fact]
    public async Task ValidateAsync_WhenTagKeyEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(tags: [new IamTag(string.Empty, "dev")]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.ErrorMessage == "Tag key must not be empty.");
    }
}
