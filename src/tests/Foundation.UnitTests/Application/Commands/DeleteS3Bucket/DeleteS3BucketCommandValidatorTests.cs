using Foundation.Application.Commands.DeleteS3Bucket;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteS3Bucket;

public class DeleteS3BucketCommandValidatorTests
{
    private readonly DeleteS3BucketCommandValidator _sut =
        new(NullLogger<DeleteS3BucketCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteS3BucketCommand("orders"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenBucketNameEmpty_ReturnsErrorForBucketName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteS3BucketCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteS3BucketCommand.BucketName));
    }
}
