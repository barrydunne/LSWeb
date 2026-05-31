using Foundation.Application.Commands.DetachUserPolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DetachUserPolicy;

public class DetachUserPolicyCommandValidatorTests
{
    private readonly DetachUserPolicyCommandValidator _sut =
        new(NullLogger<DetachUserPolicyCommandValidator>.Instance);

    private static DetachUserPolicyCommand Valid(
        string userName = "alice", string policyArn = "arn:aws:iam::aws:policy/ReadOnlyAccess")
        => new(userName, policyArn);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DetachUserPolicyCommand.UserName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyArnEmpty_ReturnsErrorForPolicyArn()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DetachUserPolicyCommand.PolicyArn));
    }

    [Theory]
    [InlineData("ReadOnlyAccess")]
    [InlineData("aws:iam::policy")]
    public async Task ValidateAsync_WhenPolicyArnNotArn_ReturnsErrorForPolicyArn(string policyArn)
    {
        var result = await _sut.ValidateAsync(
            Valid(policyArn: policyArn), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DetachUserPolicyCommand.PolicyArn));
    }
}
