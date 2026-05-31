using Foundation.Application.Commands.CreateUser;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateUser;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _sut =
        new(NullLogger<CreateUserCommandValidator>.Instance);

    private static CreateUserCommand Valid(string name = "alice", string? path = null)
        => new(name, path);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("alice")]
    [InlineData("Alice-123")]
    [InlineData("user+tag=a.b@c_d-e")]
    public async Task ValidateAsync_WhenNameUsesAllowedCharacters_IsValid(string name)
    {
        var result = await _sut.ValidateAsync(Valid(name: name), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/team/")]
    [InlineData("/team/sub/")]
    public async Task ValidateAsync_WhenPathValid_IsValid(string path)
    {
        var result = await _sut.ValidateAsync(Valid(path: path), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenNameEmpty_ReturnsErrorForUserName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateUserCommand.UserName));
    }

    [Fact]
    public async Task ValidateAsync_WhenNameTooLong_ReturnsErrorForUserName()
    {
        var result = await _sut.ValidateAsync(
            Valid(name: new string('a', 65)), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateUserCommand.UserName));
    }

    [Theory]
    [InlineData("bad name")]
    [InlineData("bad/name")]
    [InlineData("bad!name")]
    public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ReturnsErrorForUserName(string name)
    {
        var result = await _sut.ValidateAsync(Valid(name: name), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateUserCommand.UserName));
    }

    [Theory]
    [InlineData("team/")]
    [InlineData("/team")]
    [InlineData("team")]
    public async Task ValidateAsync_WhenPathInvalid_ReturnsErrorForPath(string path)
    {
        var result = await _sut.ValidateAsync(Valid(path: path), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateUserCommand.Path));
    }
}
