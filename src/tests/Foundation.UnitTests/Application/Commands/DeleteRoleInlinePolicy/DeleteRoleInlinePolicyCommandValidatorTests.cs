using Foundation.Application.Commands.DeleteRoleInlinePolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteRoleInlinePolicy;

public class DeleteRoleInlinePolicyCommandValidatorTests
{
    private readonly DeleteRoleInlinePolicyCommandValidator _sut =
        new(NullLogger<DeleteRoleInlinePolicyCommandValidator>.Instance);

    private static DeleteRoleInlinePolicyCommand Valid(
        string roleName = "deploy-role", string policyName = "inline")
        => new(roleName, policyName);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteRoleInlinePolicyCommand.RoleName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyNameEmpty_ReturnsErrorForPolicyName()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteRoleInlinePolicyCommand.PolicyName));
    }
}
