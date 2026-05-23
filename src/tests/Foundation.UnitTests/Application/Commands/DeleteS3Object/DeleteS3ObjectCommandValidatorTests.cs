using Foundation.Application.Commands.DeleteS3Object;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteS3Object;

public class DeleteS3ObjectCommandValidatorTests
{
    private readonly DeleteS3ObjectCommandValidator _sut =
        new(NullLogger<DeleteS3ObjectCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new DeleteS3ObjectCommand("data", "orders/readme.txt"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenBucketNameEmpty_ReturnsErrorForBucketName()
    {
        var result = await _sut.ValidateAsync(
            new DeleteS3ObjectCommand(string.Empty, "orders/readme.txt"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteS3ObjectCommand.BucketName));
    }

    [Fact]
    public async Task ValidateAsync_WhenKeyEmpty_ReturnsErrorForKey()
    {
        var result = await _sut.ValidateAsync(
            new DeleteS3ObjectCommand("data", string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(DeleteS3ObjectCommand.Key));
    }
}
