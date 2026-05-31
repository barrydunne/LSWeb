using Foundation.Application.Commands.SetDefaultPolicyVersion;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetDefaultPolicyVersion;

public class SetDefaultPolicyVersionCommandValidatorTests
{
    private const string Arn = "arn:aws:iam::000000000000:policy/deploy-policy";

    private readonly SetDefaultPolicyVersionCommandValidator _sut =
        new(NullLogger<SetDefaultPolicyVersionCommandValidator>.Instance);

    private static SetDefaultPolicyVersionCommand Valid(string policyArn = Arn, string versionId = "v2")
        => new(policyArn, versionId);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("v1")]
    [InlineData("v2")]
    [InlineData("v10")]
    public async Task ValidateAsync_WhenVersionIdValid_IsValid(string versionId)
    {
        var result = await _sut.ValidateAsync(Valid(versionId: versionId), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenArnEmpty_ReturnsErrorForPolicyArn()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetDefaultPolicyVersionCommand.PolicyArn));
    }

    [Fact]
    public async Task ValidateAsync_WhenVersionIdEmpty_ReturnsErrorForVersionId()
    {
        var result = await _sut.ValidateAsync(
            Valid(versionId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetDefaultPolicyVersionCommand.VersionId));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("v0")]
    [InlineData("version1")]
    [InlineData("v1a")]
    public async Task ValidateAsync_WhenVersionIdInvalid_ReturnsErrorForVersionId(string versionId)
    {
        var result = await _sut.ValidateAsync(Valid(versionId: versionId), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(SetDefaultPolicyVersionCommand.VersionId));
    }
}
