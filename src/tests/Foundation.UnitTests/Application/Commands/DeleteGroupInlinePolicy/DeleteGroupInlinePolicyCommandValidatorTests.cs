using Foundation.Application.Commands.DeleteGroupInlinePolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteGroupInlinePolicy;

public class DeleteGroupInlinePolicyCommandValidatorTests
{
    private readonly DeleteGroupInlinePolicyCommandValidator _sut =
        new(NullLogger<DeleteGroupInlinePolicyCommandValidator>.Instance);

    private static DeleteGroupInlinePolicyCommand Valid(
        string groupName = "developers", string policyName = "inline")
        => new(groupName, policyName);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenGroupNameEmpty_ReturnsErrorForGroupName()
    {
        var result = await _sut.ValidateAsync(
            Valid(groupName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteGroupInlinePolicyCommand.GroupName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyNameEmpty_ReturnsErrorForPolicyName()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteGroupInlinePolicyCommand.PolicyName));
    }
}
