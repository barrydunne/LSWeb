using Foundation.Application.Commands.CreateS3Bucket;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateS3Bucket;

public class CreateS3BucketCommandValidatorTests
{
    private readonly CreateS3BucketCommandValidator _sut =
        new(NullLogger<CreateS3BucketCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new CreateS3BucketCommand("orders"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenBucketNameEmpty_ReturnsErrorForBucketName()
    {
        var result = await _sut.ValidateAsync(
            new CreateS3BucketCommand(string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateS3BucketCommand.BucketName));
    }
}
