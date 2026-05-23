using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.GetS3ObjectMetadata;
using Foundation.Application.S3;
using Foundation.Domain.S3;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetS3ObjectMetadata;

public class GetS3ObjectMetadataQueryHandlerTests
{
    private readonly IS3Client _client = Substitute.For<IS3Client>();

    private GetS3ObjectMetadataQueryHandler CreateSut()
        => new(_client, NullLogger<GetS3ObjectMetadataQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenMetadataSucceeds_ReturnsSortedMetadataAndTags()
    {
        // Arrange
        var metadata = new S3ObjectMetadata(
            "text/plain",
            42,
            "2026-01-02T03:04:05.0000000Z",
            "\"abc123\"",
            new Dictionary<string, string> { ["owner"] = "alice", ["author"] = "bob" },
            new Dictionary<string, string> { ["stage"] = "prod", ["env"] = "live" });
        _client
            .GetObjectMetadataAsync("data", "orders/readme.txt", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3ObjectMetadata>>(metadata));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetS3ObjectMetadataQuery("data", "orders/readme.txt"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("text/plain");
        result.Value.ContentLength.Should().Be(42);
        result.Value.LastModified.Should().Be("2026-01-02T03:04:05.0000000Z");
        result.Value.ETag.Should().Be("\"abc123\"");
        result.Value.Metadata.Should().Equal(
            new S3MetadataEntry("author", "bob"),
            new S3MetadataEntry("owner", "alice"));
        result.Value.Tags.Should().Equal(
            new S3MetadataEntry("env", "live"),
            new S3MetadataEntry("stage", "prod"));
        await _client.Received(1).GetObjectMetadataAsync("data", "orders/readme.txt", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMetadataEmpty_ReturnsEmptyCollections()
    {
        // Arrange
        var metadata = new S3ObjectMetadata(
            "application/octet-stream",
            0,
            string.Empty,
            string.Empty,
            new Dictionary<string, string>(),
            new Dictionary<string, string>());
        _client
            .GetObjectMetadataAsync("data", "empty.bin", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3ObjectMetadata>>(metadata));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetS3ObjectMetadataQuery("data", "empty.bin"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Metadata.Should().BeEmpty();
        result.Value.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenMetadataFails_ReturnsError()
    {
        // Arrange
        _client
            .GetObjectMetadataAsync("data", "missing.txt", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<S3ObjectMetadata>>(new Error("metadata boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetS3ObjectMetadataQuery("data", "missing.txt"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("metadata boom");
    }
}
