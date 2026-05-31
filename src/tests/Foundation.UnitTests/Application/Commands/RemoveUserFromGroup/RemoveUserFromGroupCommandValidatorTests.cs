using Foundation.Application.Commands.RemoveUserFromGroup;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RemoveUserFromGroup;

public class RemoveUserFromGroupCommandValidatorTests
{
    private readonly RemoveUserFromGroupCommandValidator _sut =
        new(NullLogger<RemoveUserFromGroupCommandValidator>.Instance);

    private static RemoveUserFromGroupCommand Valid(string userName = "alice", string groupName = "admins")
        => new(userName, groupName);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RemoveUserFromGroupCommand.UserName));
    }

    [Fact]
    public async Task ValidateAsync_WhenGroupNameEmpty_ReturnsErrorForGroupName()
    {
        var result = await _sut.ValidateAsync(
            Valid(groupName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(RemoveUserFromGroupCommand.GroupName));
    }
}
