using Foundation.Application.Commands.UntagPolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UntagPolicy;

public class UntagPolicyCommandValidatorTests
{
    private readonly UntagPolicyCommandValidator _sut =
        new(NullLogger<UntagPolicyCommandValidator>.Instance);

    private static UntagPolicyCommand Valid(
        string policyArn = "arn:aws:iam::aws:policy/ReadOnly", IReadOnlyList<string>? tagKeys = null)
        => new(policyArn, tagKeys ?? ["team"]);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyArnEmpty_ReturnsErrorForPolicyArn()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UntagPolicyCommand.PolicyArn));
    }

    [Theory]
    [InlineData("ReadOnly")]
    [InlineData("aws:iam::policy")]
    public async Task ValidateAsync_WhenPolicyArnNotArn_ReturnsErrorForPolicyArn(string policyArn)
    {
        var result = await _sut.ValidateAsync(
            Valid(policyArn: policyArn), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UntagPolicyCommand.PolicyArn));
    }

    [Fact]
    public async Task ValidateAsync_WhenTagKeysEmpty_ReturnsErrorForTagKeys()
    {
        var result = await _sut.ValidateAsync(
            Valid(tagKeys: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UntagPolicyCommand.TagKeys));
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
