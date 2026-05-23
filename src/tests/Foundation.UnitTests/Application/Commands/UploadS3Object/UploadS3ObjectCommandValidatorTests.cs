using Foundation.Application.Commands.UploadS3Object;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UploadS3Object;

public class UploadS3ObjectCommandValidatorTests
{
    private readonly UploadS3ObjectCommandValidator _sut =
        new(NullLogger<UploadS3ObjectCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        using var content = new MemoryStream([1]);
        var result = await _sut.ValidateAsync(
            new UploadS3ObjectCommand("data", "orders/readme.txt", content, "text/plain"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenBucketNameEmpty_ReturnsErrorForBucketName()
    {
        using var content = new MemoryStream([1]);
        var result = await _sut.ValidateAsync(
            new UploadS3ObjectCommand(string.Empty, "orders/readme.txt", content, "text/plain"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UploadS3ObjectCommand.BucketName));
    }

    [Fact]
    public async Task ValidateAsync_WhenKeyEmpty_ReturnsErrorForKey()
    {
        using var content = new MemoryStream([1]);
        var result = await _sut.ValidateAsync(
            new UploadS3ObjectCommand("data", string.Empty, content, "text/plain"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UploadS3ObjectCommand.Key));
    }

    [Fact]
    public async Task ValidateAsync_WhenKeyEndsWithSlash_ReturnsErrorForKey()
    {
        using var content = new MemoryStream([1]);
        var result = await _sut.ValidateAsync(
            new UploadS3ObjectCommand("data", "orders/", content, "text/plain"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UploadS3ObjectCommand.Key));
    }

    [Fact]
    public async Task ValidateAsync_WhenContentNull_ReturnsErrorForContent()
    {
        var result = await _sut.ValidateAsync(
            new UploadS3ObjectCommand("data", "orders/readme.txt", null!, "text/plain"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(UploadS3ObjectCommand.Content));
    }
}
