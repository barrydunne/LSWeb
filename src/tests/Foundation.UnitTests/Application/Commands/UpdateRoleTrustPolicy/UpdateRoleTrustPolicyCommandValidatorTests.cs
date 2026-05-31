using Foundation.Application.Commands.UpdateRoleTrustPolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateRoleTrustPolicy;

public class UpdateRoleTrustPolicyCommandValidatorTests
{
    private const string ValidDocument = "{\"Version\":\"2012-10-17\",\"Statement\":[]}";

    private readonly UpdateRoleTrustPolicyCommandValidator _sut =
        new(NullLogger<UpdateRoleTrustPolicyCommandValidator>.Instance);

    private static UpdateRoleTrustPolicyCommand Valid(
        string roleName = "deploy-role", string policyDocument = ValidDocument)
        => new(roleName, policyDocument);

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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateRoleTrustPolicyCommand.RoleName));
    }

    [Fact]
    public async Task ValidateAsync_WhenPolicyDocumentEmpty_ReturnsErrorForPolicyDocument()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyDocument: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateRoleTrustPolicyCommand.PolicyDocument));
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
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UpdateRoleTrustPolicyCommand.PolicyDocument));
    }
}
