using Foundation.Application.Commands.UntagUser;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UntagUser;

public class UntagUserCommandValidatorTests
{
    private readonly UntagUserCommandValidator _sut =
        new(NullLogger<UntagUserCommandValidator>.Instance);

    private static UntagUserCommand Valid(
        string userName = "alice", IReadOnlyList<string>? tagKeys = null)
        => new(userName, tagKeys ?? ["env"]);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UntagUserCommand.UserName));
    }

    [Fact]
    public async Task ValidateAsync_WhenTagKeysEmpty_ReturnsErrorForTagKeys()
    {
        var result = await _sut.ValidateAsync(
            Valid(tagKeys: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UntagUserCommand.TagKeys));
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
