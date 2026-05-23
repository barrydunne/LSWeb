using System.Text;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.PreviewS3Object;
using Foundation.Application.S3;
using Foundation.Domain.S3;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.PreviewS3Object;

public class PreviewS3ObjectQueryHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();

    private PreviewS3ObjectQueryHandler CreateSut()
        => new(_client, NullLogger<PreviewS3ObjectQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenTextObject_ReturnsDecodedText()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("hello world");
        _client
            .PreviewObjectAsync("data", "notes/readme.txt", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3ObjectPreview>>(
                new S3ObjectPreview(bytes, "text/plain", bytes.Length, false)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PreviewS3ObjectQuery("data", "notes/readme.txt"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Kind.Should().Be("Text");
        result.Value.ContentType.Should().Be("text/plain");
        result.Value.Truncated.Should().BeFalse();
        result.Value.TotalSize.Should().Be(bytes.Length);
        result.Value.Text.Should().Be("hello world");
        result.Value.DataUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenJsonObject_ReturnsDecodedTextAsJsonKind()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("{\"a\":1}");
        _client
            .PreviewObjectAsync("data", "config.json", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3ObjectPreview>>(
                new S3ObjectPreview(bytes, "application/json", bytes.Length, true)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PreviewS3ObjectQuery("data", "config.json"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Kind.Should().Be("Json");
        result.Value.ContentType.Should().Be("application/json");
        result.Value.Truncated.Should().BeTrue();
        result.Value.TotalSize.Should().Be(bytes.Length);
        result.Value.Text.Should().Be("{\"a\":1}");
        result.Value.DataUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenImageObject_ReturnsDataUrl()
    {
        // Arrange
        var bytes = new byte[] { 1, 2, 3 };
        _client
            .PreviewObjectAsync("data", "logo.png", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3ObjectPreview>>(
                new S3ObjectPreview(bytes, "image/png", bytes.Length, false)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PreviewS3ObjectQuery("data", "logo.png"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Kind.Should().Be("Image");
        result.Value.ContentType.Should().Be("image/png");
        result.Value.Truncated.Should().BeFalse();
        result.Value.TotalSize.Should().Be(bytes.Length);
        result.Value.Text.Should().BeNull();
        result.Value.DataUrl.Should().Be($"data:image/png;base64,{Convert.ToBase64String(bytes)}");
    }

    [Fact]
    public async Task Handle_WhenBinaryObject_ReturnsNoTextOrDataUrl()
    {
        // Arrange
        var bytes = new byte[] { 4, 5, 6 };
        _client
            .PreviewObjectAsync("data", "archive.bin", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3ObjectPreview>>(
                new S3ObjectPreview(bytes, "application/octet-stream", bytes.Length, false)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PreviewS3ObjectQuery("data", "archive.bin"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Kind.Should().Be("Binary");
        result.Value.ContentType.Should().Be("application/octet-stream");
        result.Value.Text.Should().BeNull();
        result.Value.DataUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenPreviewFails_ReturnsError()
    {
        // Arrange
        _client
            .PreviewObjectAsync("data", "missing.txt", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3ObjectPreview>>(new Error("preview boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PreviewS3ObjectQuery("data", "missing.txt"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("preview boom");
    }
}
