using Foundation.Application.Commands.CreateS3Folder;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateS3Folder;

public class CreateS3FolderCommandValidatorTests
{
    private readonly CreateS3FolderCommandValidator _sut =
        new(NullLogger<CreateS3FolderCommandValidator>.Instance);

    [Fact]
    public async Task ValidateAsync_WhenValid_IsValid()
    {
        var result = await _sut.ValidateAsync(
            new CreateS3FolderCommand("data", "orders/2026/"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenBucketNameEmpty_ReturnsErrorForBucketName()
    {
        var result = await _sut.ValidateAsync(
            new CreateS3FolderCommand(string.Empty, "orders/"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateS3FolderCommand.BucketName));
    }

    [Fact]
    public async Task ValidateAsync_WhenFolderKeyEmpty_ReturnsErrorForFolderKey()
    {
        var result = await _sut.ValidateAsync(
            new CreateS3FolderCommand("data", string.Empty), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateS3FolderCommand.FolderKey));
    }

    [Fact]
    public async Task ValidateAsync_WhenFolderKeyMissingTrailingSlash_ReturnsErrorForFolderKey()
    {
        var result = await _sut.ValidateAsync(
            new CreateS3FolderCommand("data", "orders"), TestContext.Current.CancellationToken);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(_ => _.PropertyName == nameof(CreateS3FolderCommand.FolderKey));
    }
}
