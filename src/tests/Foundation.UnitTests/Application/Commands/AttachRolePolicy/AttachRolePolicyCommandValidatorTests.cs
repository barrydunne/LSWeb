using Foundation.Application.Commands.AttachRolePolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.AttachRolePolicy;

public class AttachRolePolicyCommandValidatorTests
{
    private readonly AttachRolePolicyCommandValidator _sut =
        new(NullLogger<AttachRolePolicyCommandValidator>.Instance);

    private static AttachRolePolicyCommand Valid(
        string roleName = "deploy-role", string policyArn = "arn:aws:iam::aws:policy/ReadOnlyAccess")
        => new(roleName, policyArn);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(AttachRolePolicyCommand.RoleName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyArnEmpty_ReturnsErrorForPolicyArn()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(AttachRolePolicyCommand.PolicyArn));
    }

    [Theory]
    [InlineData("ReadOnlyAccess")]
    [InlineData("aws:iam::policy")]
    public async Task ValidateAsync_WhenPolicyArnNotArn_ReturnsErrorForPolicyArn(string policyArn)
    {
        var result = await _sut.ValidateAsync(
            Valid(policyArn: policyArn), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(AttachRolePolicyCommand.PolicyArn));
    }
}
