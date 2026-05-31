using Foundation.Application.Commands.TagPolicy;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.TagPolicy;

public class TagPolicyCommandValidatorTests
{
    private readonly TagPolicyCommandValidator _sut =
        new(NullLogger<TagPolicyCommandValidator>.Instance);

    private static TagPolicyCommand Valid(
        string policyArn = "arn:aws:iam::aws:policy/ReadOnly", IReadOnlyList<IamTag>? tags = null)
        => new(policyArn, tags ?? [new IamTag("team", "platform")]);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TagPolicyCommand.PolicyArn));
    }

    [Theory]
    [InlineData("ReadOnly")]
    [InlineData("aws:iam::policy")]
    public async Task ValidateAsync_WhenPolicyArnNotArn_ReturnsErrorForPolicyArn(string policyArn)
    {
        var result = await _sut.ValidateAsync(
            Valid(policyArn: policyArn), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TagPolicyCommand.PolicyArn));
    }

    [Fact]
    public async Task ValidateAsync_WhenTagsEmpty_ReturnsErrorForTags()
    {
        var result = await _sut.ValidateAsync(
            Valid(tags: []), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(TagPolicyCommand.Tags));
    }

    [Fact]
    public async Task ValidateAsync_WhenTagKeyEmpty_ReturnsError()
    {
        var result = await _sut.ValidateAsync(
            Valid(tags: [new IamTag(string.Empty, "platform")]), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.ErrorMessage == "Tag key must not be empty.");
    }
}
