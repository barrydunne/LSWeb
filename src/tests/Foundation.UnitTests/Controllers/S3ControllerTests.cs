using System.Text;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.CopyS3Object;
using Foundation.Application.Commands.CreateS3Bucket;
using Foundation.Application.Commands.CreateS3Folder;
using Foundation.Application.Commands.DeleteS3Bucket;
using Foundation.Application.Commands.DeleteS3Object;
using Foundation.Application.Commands.MoveS3Object;
using Foundation.Application.Commands.UpdateS3ObjectTags;
using Foundation.Application.Commands.UploadS3Object;
using Foundation.Application.Queries.DownloadS3Object;
using Foundation.Application.Queries.GetS3BucketConfiguration;
using Foundation.Application.Queries.GetS3BucketStorageSummary;
using Foundation.Application.Queries.GetS3ObjectMetadata;
using Foundation.Application.Queries.ListS3Buckets;
using Foundation.Application.Queries.ListS3Objects;
using Foundation.Application.Queries.PresignS3Object;
using Foundation.Application.Queries.PreviewS3Object;
using Foundation.Domain.S3;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class S3ControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<S3Controller> _logger = Substitute.For<ILogger<S3Controller>>();

    private S3Controller CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListBuckets_WhenQuerySucceeds_ReturnsOkWithSummaries()
    {
        // Arrange
        IReadOnlyList<S3Bucket> buckets =
        [
            new("orders", "2026-01-02T03:04:05.0000000Z"),
        ];
        _sender
            .Send(Arg.Any<ListS3BucketsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListS3BucketsQueryResult>>(
                new ListS3BucketsQueryResult(buckets)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListBuckets(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<S3BucketListResponse>>().Subject;
        var summary = ok.Value!.Buckets.Should().ContainSingle().Subject;
        summary.Name.Should().Be("orders");
        summary.CreationDate.Should().Be("2026-01-02T03:04:05.0000000Z");
    }

    [Fact]
    public async Task ListBuckets_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListS3BucketsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListS3BucketsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListBuckets(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateBucket_WhenCommandSucceeds_ReturnsCreated()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateS3BucketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateBucket(
            new S3BucketCreateRequest("orders"), TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created>().Subject;
        created.Location.Should().Be("/api/services/s3/buckets/orders");
    }

    [Fact]
    public async Task CreateBucket_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateS3BucketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateBucket(
            new S3BucketCreateRequest("orders"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteBucket_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteS3BucketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteBucket("orders", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task DeleteBucket_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteS3BucketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteBucket("orders", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListObjects_WhenQuerySucceeds_ReturnsOkWithListing()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListS3ObjectsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListS3ObjectsQueryResult>>(
                new ListS3ObjectsQueryResult(
                    ["orders/2026/"],
                    [new S3Object("orders/readme.txt", 12, "2026-01-02T03:04:05.0000000Z")])));
        var sut = CreateSut();

        // Act
        var result = await sut.ListObjects("data", "orders/", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<S3ObjectListingResponse>>().Subject;
        ok.Value!.Prefixes.Should().ContainSingle(_ => _ == "orders/2026/");
        var item = ok.Value.Objects.Should().ContainSingle().Subject;
        item.Key.Should().Be("orders/readme.txt");
        item.Size.Should().Be(12);
        item.LastModified.Should().Be("2026-01-02T03:04:05.0000000Z");
    }

    [Fact]
    public async Task ListObjects_WhenPrefixOmitted_UsesEmptyPrefix()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListS3ObjectsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListS3ObjectsQueryResult>>(
                new ListS3ObjectsQueryResult([], [])));
        var sut = CreateSut();

        // Act
        var result = await sut.ListObjects("data", null, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Ok<S3ObjectListingResponse>>();
        await _sender.Received(1).Send(
            Arg.Is<ListS3ObjectsQuery>(query => query.Prefix == string.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListObjects_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListS3ObjectsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListS3ObjectsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListObjects("data", "orders/", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CreateFolder_WhenCommandSucceeds_ReturnsCreated()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateS3FolderCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateFolder(
            "data", new S3FolderCreateRequest("orders/2026/"), TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created>().Subject;
        created.Location.Should().Be("/api/services/s3/buckets/data/objects?prefix=orders%2F2026%2F");
    }

    [Fact]
    public async Task CreateFolder_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CreateS3FolderCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateFolder(
            "data", new S3FolderCreateRequest("orders/2026/"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UploadObject_WhenCommandSucceeds_ReturnsCreated()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UploadS3ObjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();
        var request = CreateUploadRequest("readme.txt", "orders/", "text/plain", "hello");

        // Act
        var result = await sut.UploadObject("data", request, TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created>().Subject;
        created.Location.Should().Be("/api/services/s3/buckets/data/objects?prefix=orders%2F");
        await _sender.Received(1).Send(
            Arg.Is<UploadS3ObjectCommand>(command =>
                command.Key == "orders/readme.txt" && command.ContentType == "text/plain"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadObject_WhenContentTypeMissing_UsesOctetStreamAndRootPrefix()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UploadS3ObjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();
        var request = CreateUploadRequest("data.bin", null, null, "x");

        // Act
        var result = await sut.UploadObject("data", request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Created>();
        await _sender.Received(1).Send(
            Arg.Is<UploadS3ObjectCommand>(command =>
                command.Key == "data.bin" && command.ContentType == "application/octet-stream"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadObject_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UploadS3ObjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();
        var request = CreateUploadRequest("readme.txt", "orders/", "text/plain", "hello");

        // Act
        var result = await sut.UploadObject("data", request, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DownloadObject_WhenQuerySucceeds_ReturnsFile()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DownloadS3ObjectQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DownloadS3ObjectQueryResult>>(
                new DownloadS3ObjectQueryResult([1, 2, 3], "text/plain", "readme.txt")));
        var sut = CreateSut();

        // Act
        var result = await sut.DownloadObject("data", "orders/readme.txt", TestContext.Current.CancellationToken);

        // Assert
        var file = result.Should().BeOfType<FileContentHttpResult>().Subject;
        file.ContentType.Should().Be("text/plain");
        file.FileDownloadName.Should().Be("readme.txt");
        file.FileContents.ToArray().Should().Equal((byte)1, (byte)2, (byte)3);
    }

    [Fact]
    public async Task DownloadObject_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DownloadS3ObjectQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DownloadS3ObjectQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DownloadObject("data", "orders/readme.txt", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PreviewObject_WhenQuerySucceeds_ReturnsOkWithMappedFields()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PreviewS3ObjectQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<PreviewS3ObjectQueryResult>>(
                new PreviewS3ObjectQueryResult("Json", "application/json", true, 1024, "{\"a\":1}", null)));
        var sut = CreateSut();

        // Act
        var result = await sut.PreviewObject("data", "config.json", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<S3ObjectPreviewResponse>>().Subject;
        ok.Value!.Kind.Should().Be("Json");
        ok.Value.ContentType.Should().Be("application/json");
        ok.Value.Truncated.Should().BeTrue();
        ok.Value.TotalSize.Should().Be(1024);
        ok.Value.Text.Should().Be("{\"a\":1}");
        ok.Value.DataUrl.Should().BeNull();
    }

    [Fact]
    public async Task PreviewObject_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PreviewS3ObjectQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<PreviewS3ObjectQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PreviewObject("data", "config.json", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PresignObject_WhenQuerySucceeds_ReturnsOkWithUrlAndExpiry()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PresignS3ObjectQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<PresignS3ObjectQueryResult>>(
                new PresignS3ObjectQueryResult("https://example.test/presigned", 900)));
        var sut = CreateSut();

        // Act
        var result = await sut.PresignObject("data", "config.json", 900, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<S3PresignedUrlResponse>>().Subject;
        ok.Value!.Url.Should().Be("https://example.test/presigned");
        ok.Value.ExpirySeconds.Should().Be(900);
    }

    [Fact]
    public async Task PresignObject_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PresignS3ObjectQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<PresignS3ObjectQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PresignObject("data", "config.json", 900, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteObject_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteS3ObjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteObject("data", "orders/readme.txt", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task DeleteObject_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteS3ObjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteObject("data", "orders/readme.txt", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetObjectMetadata_WhenQuerySucceeds_ReturnsOkWithMappedFields()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetS3ObjectMetadataQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetS3ObjectMetadataQueryResult>>(
                new GetS3ObjectMetadataQueryResult(
                    "text/plain",
                    42,
                    "2026-01-02T03:04:05.0000000Z",
                    "\"abc123\"",
                    [new S3MetadataEntry("owner", "alice")],
                    [new S3MetadataEntry("stage", "prod")])));
        var sut = CreateSut();

        // Act
        var result = await sut.GetObjectMetadata("data", "orders/readme.txt", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<S3ObjectMetadataResponse>>().Subject;
        ok.Value!.ContentType.Should().Be("text/plain");
        ok.Value.ContentLength.Should().Be(42);
        ok.Value.LastModified.Should().Be("2026-01-02T03:04:05.0000000Z");
        ok.Value.ETag.Should().Be("\"abc123\"");
        ok.Value.Metadata.Should().ContainSingle(_ => _.Key == "owner" && _.Value == "alice");
        ok.Value.Tags.Should().ContainSingle(_ => _.Key == "stage" && _.Value == "prod");
    }

    [Fact]
    public async Task GetObjectMetadata_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetS3ObjectMetadataQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetS3ObjectMetadataQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetObjectMetadata("data", "orders/readme.txt", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateObjectTags_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateS3ObjectTagsCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateObjectTags(
            "data",
            "orders/readme.txt",
            new S3ObjectTagsUpdateRequest(new Dictionary<string, string> { ["stage"] = "prod" }),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task UpdateObjectTags_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<UpdateS3ObjectTagsCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateObjectTags(
            "data",
            "orders/readme.txt",
            new S3ObjectTagsUpdateRequest(new Dictionary<string, string> { ["stage"] = "prod" }),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task CopyObject_WhenCommandSucceeds_ReturnsCreatedAtDestinationPrefix()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CopyS3ObjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CopyObject(
            "data",
            "orders/readme.txt",
            new S3ObjectCopyRequest("archive", "orders/2026/readme.txt"),
            TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created>().Subject;
        created.Location.Should().Be("/api/services/s3/buckets/archive/objects?prefix=orders%2F2026%2F");
        await _sender.Received(1).Send(
            Arg.Is<CopyS3ObjectCommand>(command =>
                command.SourceBucketName == "data"
                && command.SourceKey == "orders/readme.txt"
                && command.DestinationBucketName == "archive"
                && command.DestinationKey == "orders/2026/readme.txt"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CopyObject_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<CopyS3ObjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CopyObject(
            "data",
            "orders/readme.txt",
            new S3ObjectCopyRequest("archive", "orders/2026/readme.txt"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task MoveObject_WhenCommandSucceeds_ReturnsCreatedAtRootPrefixForUnprefixedKey()
    {
        // Arrange
        _sender
            .Send(Arg.Any<MoveS3ObjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.MoveObject(
            "data",
            "orders/readme.txt",
            new S3ObjectCopyRequest("data", "readme.txt"),
            TestContext.Current.CancellationToken);

        // Assert
        var created = result.Should().BeOfType<Created>().Subject;
        created.Location.Should().Be("/api/services/s3/buckets/data/objects?prefix=");
        await _sender.Received(1).Send(
            Arg.Is<MoveS3ObjectCommand>(command =>
                command.SourceBucketName == "data"
                && command.SourceKey == "orders/readme.txt"
                && command.DestinationBucketName == "data"
                && command.DestinationKey == "readme.txt"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MoveObject_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<MoveS3ObjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.MoveObject(
            "data",
            "orders/readme.txt",
            new S3ObjectCopyRequest("archive", "orders/2026/readme.txt"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetBucketConfiguration_WhenQuerySucceeds_ReturnsOkWithMappedFields()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetS3BucketConfigurationQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetS3BucketConfigurationQueryResult>>(
                new GetS3BucketConfigurationQueryResult(
                    "Enabled",
                    "aws:kms",
                    "key-1",
                    [new S3LifecycleRuleResult("expire", "Enabled", "logs/")],
                    [new S3NotificationResult("Lambda", "arn:aws:lambda:us-east-1:000000000000:function:p", ["s3:ObjectCreated:*"], "uploads/", ".json")],
                    "{\"Version\":\"2012-10-17\"}")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetBucketConfiguration("data", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<S3BucketConfigurationResponse>>().Subject;
        ok.Value!.VersioningStatus.Should().Be("Enabled");
        ok.Value.EncryptionAlgorithm.Should().Be("aws:kms");
        ok.Value.EncryptionKeyId.Should().Be("key-1");
        ok.Value.Policy.Should().Be("{\"Version\":\"2012-10-17\"}");
        var rule = ok.Value.LifecycleRules.Should().ContainSingle().Subject;
        rule.Id.Should().Be("expire");
        rule.Status.Should().Be("Enabled");
        rule.Prefix.Should().Be("logs/");
        var notification = ok.Value.Notifications.Should().ContainSingle().Subject;
        notification.Type.Should().Be("Lambda");
        notification.TargetArn.Should().Be("arn:aws:lambda:us-east-1:000000000000:function:p");
        notification.Events.Should().Equal("s3:ObjectCreated:*");
        notification.Prefix.Should().Be("uploads/");
        notification.Suffix.Should().Be(".json");
    }

    [Fact]
    public async Task GetBucketConfiguration_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetS3BucketConfigurationQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetS3BucketConfigurationQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetBucketConfiguration("data", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetBucketStorageSummary_WhenQuerySucceeds_ReturnsOkWithMappedFields()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetS3BucketStorageSummaryQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetS3BucketStorageSummaryQueryResult>>(
                new GetS3BucketStorageSummaryQueryResult(9, 8192)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetBucketStorageSummary("data", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<S3BucketStorageSummaryResponse>>().Subject;
        ok.Value!.ObjectCount.Should().Be(9);
        ok.Value.TotalSizeBytes.Should().Be(8192);
    }

    [Fact]
    public async Task GetBucketStorageSummary_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetS3BucketStorageSummaryQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetS3BucketStorageSummaryQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetBucketStorageSummary("data", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    private static S3ObjectUploadRequest CreateUploadRequest(
        string fileName, string? prefix, string? contentType, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
        };
        if (contentType is not null)
            file.ContentType = contentType;

        return new S3ObjectUploadRequest { File = file, Prefix = prefix };
    }
}
