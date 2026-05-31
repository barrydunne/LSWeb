using Foundation.Application.Commands.PutUserInlinePolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutUserInlinePolicy;

public class PutUserInlinePolicyCommandValidatorTests
{
    private readonly PutUserInlinePolicyCommandValidator _sut =
        new(NullLogger<PutUserInlinePolicyCommandValidator>.Instance);

    private static PutUserInlinePolicyCommand Valid(
        string userName = "alice",
        string policyName = "inline",
        string policyDocument = "{\"Version\":\"2012-10-17\"}")
        => new(userName, policyName, policyDocument);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutUserInlinePolicyCommand.UserName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyNameEmpty_ReturnsErrorForPolicyName()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutUserInlinePolicyCommand.PolicyName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyDocumentEmpty_ReturnsErrorForPolicyDocument()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyDocument: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutUserInlinePolicyCommand.PolicyDocument));
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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutUserInlinePolicyCommand.PolicyDocument));
    }
}
