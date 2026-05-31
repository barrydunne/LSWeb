using Foundation.Application.Commands.PutGroupInlinePolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutGroupInlinePolicy;

public class PutGroupInlinePolicyCommandValidatorTests
{
    private const string ValidDocument = "{\"Version\":\"2012-10-17\"}";

    private readonly PutGroupInlinePolicyCommandValidator _sut =
        new(NullLogger<PutGroupInlinePolicyCommandValidator>.Instance);

    private static PutGroupInlinePolicyCommand Valid(
        string groupName = "developers",
        string policyName = "inline",
        string policyDocument = ValidDocument)
        => new(groupName, policyName, policyDocument);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutGroupInlinePolicyCommand.GroupName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyNameEmpty_ReturnsErrorForPolicyName()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutGroupInlinePolicyCommand.PolicyName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyDocumentEmpty_ReturnsErrorForPolicyDocument()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyDocument: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutGroupInlinePolicyCommand.PolicyDocument));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    public async Task ValidateAsync_WhenPolicyDocumentNotJsonObject_ReturnsErrorForPolicyDocument(string document)
    {
        var result = await _sut.ValidateAsync(
            Valid(policyDocument: document), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutGroupInlinePolicyCommand.PolicyDocument));
    }
}
