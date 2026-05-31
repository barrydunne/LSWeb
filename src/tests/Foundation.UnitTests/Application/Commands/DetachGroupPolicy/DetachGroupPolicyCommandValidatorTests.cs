using Foundation.Application.Commands.DetachGroupPolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DetachGroupPolicy;

public class DetachGroupPolicyCommandValidatorTests
{
    private readonly DetachGroupPolicyCommandValidator _sut =
        new(NullLogger<DetachGroupPolicyCommandValidator>.Instance);

    private static DetachGroupPolicyCommand Valid(
        string groupName = "developers", string policyArn = "arn:aws:iam::aws:policy/ReadOnlyAccess")
        => new(groupName, policyArn);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DetachGroupPolicyCommand.GroupName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyArnEmpty_ReturnsErrorForPolicyArn()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DetachGroupPolicyCommand.PolicyArn));
    }

    [Theory]
    [InlineData("ReadOnlyAccess")]
    [InlineData("aws:iam::policy")]
    public async Task ValidateAsync_WhenPolicyArnNotArn_ReturnsErrorForPolicyArn(string policyArn)
    {
        var result = await _sut.ValidateAsync(
            Valid(policyArn: policyArn), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DetachGroupPolicyCommand.PolicyArn));
    }
}
