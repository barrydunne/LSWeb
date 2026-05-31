using Foundation.Application.Commands.DeleteUserInlinePolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteUserInlinePolicy;

public class DeleteUserInlinePolicyCommandValidatorTests
{
    private readonly DeleteUserInlinePolicyCommandValidator _sut =
        new(NullLogger<DeleteUserInlinePolicyCommandValidator>.Instance);

    private static DeleteUserInlinePolicyCommand Valid(string userName = "alice", string policyName = "inline")
        => new(userName, policyName);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteUserInlinePolicyCommand.UserName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyNameEmpty_ReturnsErrorForPolicyName()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteUserInlinePolicyCommand.PolicyName));
    }
}
