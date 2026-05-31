using Foundation.Application.Commands.AddUserToGroup;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.AddUserToGroup;

public class AddUserToGroupCommandValidatorTests
{
    private readonly AddUserToGroupCommandValidator _sut =
        new(NullLogger<AddUserToGroupCommandValidator>.Instance);

    private static AddUserToGroupCommand Valid(string userName = "alice", string groupName = "admins")
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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(AddUserToGroupCommand.UserName));
    }

    [Fact]
    public async Task ValidateAsync_WhenGroupNameEmpty_ReturnsErrorForGroupName()
    {
        var result = await _sut.ValidateAsync(
            Valid(groupName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(AddUserToGroupCommand.GroupName));
    }
}
