using Foundation.Application.Commands.CopyS3Object;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CopyS3Object;

public class CopyS3ObjectCommandValidatorTests
{
    private readonly CopyS3ObjectCommandValidator _sut =
        new(NullLogger<CopyS3ObjectCommandValidator>.Instance);

    private static CopyS3ObjectCommand Valid()
        => new("data", "orders/readme.txt", "archive", "orders/2026/readme.txt");

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(Valid(), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenSameBucketDifferentKey_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new CopyS3ObjectCommand("data", "orders/readme.txt", "data", "orders/copy.txt"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenSourceBucketNameEmpty_ReturnsErrorForSourceBucketName()
    {
        var result = await _sut.ValidateAsync(
            new CopyS3ObjectCommand(string.Empty, "orders/readme.txt", "archive", "orders/2026/readme.txt"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CopyS3ObjectCommand.SourceBucketName));
    }

    [Fact]
    public async Task ValidateAsync_WhenSourceKeyEmpty_ReturnsErrorForSourceKey()
    {
        var result = await _sut.ValidateAsync(
            new CopyS3ObjectCommand("data", string.Empty, "archive", "orders/2026/readme.txt"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CopyS3ObjectCommand.SourceKey));
    }

    [Fact]
    public async Task ValidateAsync_WhenDestinationBucketNameEmpty_ReturnsErrorForDestinationBucketName()
    {
        var result = await _sut.ValidateAsync(
            new CopyS3ObjectCommand("data", "orders/readme.txt", string.Empty, "orders/2026/readme.txt"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CopyS3ObjectCommand.DestinationBucketName));
    }

    [Fact]
    public async Task ValidateAsync_WhenDestinationKeyEmpty_ReturnsErrorForDestinationKey()
    {
        var result = await _sut.ValidateAsync(
            new CopyS3ObjectCommand("data", "orders/readme.txt", "archive", string.Empty),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CopyS3ObjectCommand.DestinationKey));
    }

    [Fact]
    public async Task ValidateAsync_WhenSourceAndDestinationIdentical_ReturnsErrorForDestinationKey()
    {
        var result = await _sut.ValidateAsync(
            new CopyS3ObjectCommand("data", "orders/readme.txt", "data", "orders/readme.txt"),
            TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ =>
            _.PropertyName == nameof(CopyS3ObjectCommand.DestinationKey)
            && _.ErrorMessage == "Source and destination must be different.");
    }
}
