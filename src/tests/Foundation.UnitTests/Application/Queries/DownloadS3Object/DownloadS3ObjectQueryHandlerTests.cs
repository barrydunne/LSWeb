using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.DownloadS3Object;
using Foundation.Application.S3;
using Foundation.Domain.S3;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.DownloadS3Object;

public class DownloadS3ObjectQueryHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();

    private DownloadS3ObjectQueryHandler CreateSut()
        => new(_client, NullLogger<DownloadS3ObjectQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenDownloadSucceeds_ReturnsContentWithFileNameFromKey()
    {
        // Arrange
        _client
            .DownloadObjectAsync("data", "orders/2026/readme.txt", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3ObjectContent>>(
                new S3ObjectContent([1, 2, 3], "text/plain")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DownloadS3ObjectQuery("data", "orders/2026/readme.txt"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Equal((byte)1, (byte)2, (byte)3);
        result.Value.ContentType.Should().Be("text/plain");
        result.Value.FileName.Should().Be("readme.txt");
    }

    [Fact]
    public async Task Handle_WhenKeyHasNoDelimiter_UsesKeyAsFileName()
    {
        // Arrange
        _client
            .DownloadObjectAsync("data", "readme.txt", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3ObjectContent>>(
                new S3ObjectContent([9], "application/octet-stream")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DownloadS3ObjectQuery("data", "readme.txt"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("readme.txt");
    }

    [Fact]
    public async Task Handle_WhenKeyEndsWithDelimiter_FallsBackToFullKey()
    {
        // Arrange
        _client
            .DownloadObjectAsync("data", "orders/", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3ObjectContent>>(
                new S3ObjectContent([0], "application/octet-stream")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DownloadS3ObjectQuery("data", "orders/"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("orders/");
    }

    [Fact]
    public async Task Handle_WhenDownloadFails_ReturnsError()
    {
        // Arrange
        _client
            .DownloadObjectAsync("data", "orders/readme.txt", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3ObjectContent>>(new Error("download boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DownloadS3ObjectQuery("data", "orders/readme.txt"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("download boom");
    }
}
