using Foundation.Application.Commands.DeletePolicyVersion;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeletePolicyVersion;

public class DeletePolicyVersionCommandValidatorTests
{
    private const string Arn = "arn:aws:iam::000000000000:policy/deploy-policy";

    private readonly DeletePolicyVersionCommandValidator _sut =
        new(NullLogger<DeletePolicyVersionCommandValidator>.Instance);

    private static DeletePolicyVersionCommand Valid(string policyArn = Arn, string versionId = "v1")
        => new(policyArn, versionId);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenArnEmpty_ReturnsErrorForPolicyArn()
    {
        var result = await _sut.ValidateAsync(
            Valid(policyArn: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeletePolicyVersionCommand.PolicyArn));
    }

    [Fact]
    public async Task ValidateAsync_WhenVersionIdEmpty_ReturnsErrorForVersionId()
    {
        var result = await _sut.ValidateAsync(
            Valid(versionId: string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeletePolicyVersionCommand.VersionId));
    }
}
