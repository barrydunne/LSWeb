using Foundation.Application.Commands.PutRoleInlinePolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutRoleInlinePolicy;

public class PutRoleInlinePolicyCommandValidatorTests
{
    private const string ValidDocument = "{\"Version\":\"2012-10-17\"}";

    private readonly PutRoleInlinePolicyCommandValidator _sut =
        new(NullLogger<PutRoleInlinePolicyCommandValidator>.Instance);

    private static PutRoleInlinePolicyCommand Valid(
        string roleName = "deploy-role",
        string policyName = "inline",
        string policyDocument = ValidDocument)
        => new(roleName, policyName, policyDocument);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRoleInlinePolicyCommand.RoleName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyNameEmpty_ReturnsErrorForPolicyName()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyName: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRoleInlinePolicyCommand.PolicyName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyDocumentEmpty_ReturnsErrorForPolicyDocument()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyDocument: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRoleInlinePolicyCommand.PolicyDocument));
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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(PutRoleInlinePolicyCommand.PolicyDocument));
    }
}
