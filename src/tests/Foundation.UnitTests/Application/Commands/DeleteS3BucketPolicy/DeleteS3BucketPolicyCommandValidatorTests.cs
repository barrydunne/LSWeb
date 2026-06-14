using Foundation.Application.Commands.DeleteS3BucketPolicy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteS3BucketPolicy;

public class DeleteS3BucketPolicyCommandValidatorTests
{
    private readonly DeleteS3BucketPolicyCommandValidator _sut =
        new(NullLogger<DeleteS3BucketPolicyCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteS3BucketPolicyCommand("docs"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenBucketEmpty_ReturnsErrorForBucketName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteS3BucketPolicyCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteS3BucketPolicyCommand.BucketName));
    }
}
