using AspNet.KickStarter.FunctionalResult.Extensions;
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
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to AWS S3 buckets: listing the available buckets, creating a new bucket and
/// deleting a bucket.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/s3")]
public partial class S3Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="S3Controller"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public S3Controller(ISender sender, ILogger<S3Controller> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the S3 buckets available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the bucket summaries.</returns>
    [HttpGet("buckets")]
    [ProducesResponseType(typeof(S3BucketListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListBuckets(CancellationToken cancellationToken)
    {
        LogHandlingList();
        var result = await _sender.Send(new ListS3BucketsQuery(), cancellationToken);
        LogListHandled(result.IsSuccess);
        return result.Match(
            buckets => Results.Ok(new S3BucketListResponse(
                buckets.Buckets
                    .Select(bucket => new S3BucketResponse(
                        bucket.Name,
                        bucket.CreationDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new S3 bucket.
    /// </summary>
    /// <param name="request">The bucket to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created bucket.</returns>
    [HttpPost("buckets")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateBucket([FromBody] S3BucketCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreate(request.BucketName);
        var result = await _sender.Send(new CreateS3BucketCommand(request.BucketName), cancellationToken);
        LogCreateHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created($"/api/services/s3/buckets/{Uri.EscapeDataString(request.BucketName)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an S3 bucket.
    /// </summary>
    /// <param name="bucketName">The name of the bucket to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("buckets/{bucketName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteBucket(string bucketName, CancellationToken cancellationToken)
    {
        LogHandlingDelete(bucketName);
        var result = await _sender.Send(new DeleteS3BucketCommand(bucketName), cancellationToken);
        LogDeleteHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the folders and objects directly beneath a prefix within a bucket.
    /// </summary>
    /// <param name="bucketName">The bucket to browse.</param>
    /// <param name="prefix">The prefix to list beneath; empty lists the bucket root.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the folders and objects.</returns>
    [HttpGet("buckets/{bucketName}/objects")]
    [ProducesResponseType(typeof(S3ObjectListingResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListObjects(string bucketName, [FromQuery] string? prefix, CancellationToken cancellationToken)
    {
        LogHandlingListObjects(bucketName, prefix ?? string.Empty);
        var result = await _sender.Send(new ListS3ObjectsQuery(bucketName, prefix ?? string.Empty), cancellationToken);
        LogListObjectsHandled(result.IsSuccess);
        return result.Match(
            listing => Results.Ok(new S3ObjectListingResponse(
                listing.Prefixes,
                listing.Objects
                    .Select(o => new S3ObjectResponse(o.Key, o.Size, o.LastModified))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a zero-byte folder marker so an empty prefix appears as a navigable folder.
    /// </summary>
    /// <param name="bucketName">The bucket the folder lives in.</param>
    /// <param name="request">The folder to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created folder.</returns>
    [HttpPost("buckets/{bucketName}/folders")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateFolder(string bucketName, [FromBody] S3FolderCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateFolder(bucketName, request.FolderKey);
        var result = await _sender.Send(new CreateS3FolderCommand(bucketName, request.FolderKey), cancellationToken);
        LogCreateFolderHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/s3/buckets/{Uri.EscapeDataString(bucketName)}/objects?prefix={Uri.EscapeDataString(request.FolderKey)}",
                null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Uploads an object into a bucket beneath an optional prefix, streaming the request content.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="request">The multipart upload request carrying the file and target prefix.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the prefix the object was uploaded to.</returns>
    [HttpPost("buckets/{bucketName}/objects")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> UploadObject(
        string bucketName, [FromForm] S3ObjectUploadRequest request, CancellationToken cancellationToken)
    {
        var prefix = request.Prefix ?? string.Empty;
        var key = $"{prefix}{request.File.FileName}";
        LogHandlingUpload(bucketName, key);
        var contentType = string.IsNullOrEmpty(request.File.ContentType)
            ? "application/octet-stream"
            : request.File.ContentType;
        await using var stream = request.File.OpenReadStream();
        var result = await _sender.Send(
            new UploadS3ObjectCommand(bucketName, key, stream, contentType), cancellationToken);
        LogUploadHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/s3/buckets/{Uri.EscapeDataString(bucketName)}/objects?prefix={Uri.EscapeDataString(prefix)}",
                null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Downloads a single object from a bucket.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="key">The full key of the object within the bucket.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 file result carrying the object content.</returns>
    [HttpGet("buckets/{bucketName}/objects/content")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IResult> DownloadObject(
        string bucketName, [FromQuery] string key, CancellationToken cancellationToken)
    {
        LogHandlingDownload(bucketName, key);
        var result = await _sender.Send(new DownloadS3ObjectQuery(bucketName, key), cancellationToken);
        LogDownloadHandled(result.IsSuccess);
        return result.Match(
            content => Results.File(content.Content, content.ContentType, content.FileName),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Reads a size-limited, classified preview of a single object for inline display.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="key">The full key of the object within the bucket.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the classified preview.</returns>
    [HttpGet("buckets/{bucketName}/objects/preview")]
    [ProducesResponseType(typeof(S3ObjectPreviewResponse), StatusCodes.Status200OK)]
    public async Task<IResult> PreviewObject(
        string bucketName, [FromQuery] string key, CancellationToken cancellationToken)
    {
        LogHandlingPreview(bucketName, key);
        var result = await _sender.Send(new PreviewS3ObjectQuery(bucketName, key), cancellationToken);
        LogPreviewHandled(result.IsSuccess);
        return result.Match(
            preview => Results.Ok(new S3ObjectPreviewResponse(
                preview.Kind, preview.ContentType, preview.Truncated, preview.TotalSize, preview.Text, preview.DataUrl)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Generates a time-limited presigned GET URL for a single object.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="key">The full key of the object within the bucket.</param>
    /// <param name="expirySeconds">The requested URL lifetime in seconds; clamped to a safe range.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the presigned URL and effective expiry.</returns>
    [HttpGet("buckets/{bucketName}/objects/presign")]
    [ProducesResponseType(typeof(S3PresignedUrlResponse), StatusCodes.Status200OK)]
    public async Task<IResult> PresignObject(
        string bucketName, [FromQuery] string key, [FromQuery] int expirySeconds, CancellationToken cancellationToken)
    {
        LogHandlingPresign(bucketName, key, expirySeconds);
        var result = await _sender.Send(new PresignS3ObjectQuery(bucketName, key, expirySeconds), cancellationToken);
        LogPresignHandled(result.IsSuccess);
        return result.Match(
            presigned => Results.Ok(new S3PresignedUrlResponse(presigned.Url, presigned.ExpirySeconds)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a single object from a bucket.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="key">The full key of the object within the bucket.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("buckets/{bucketName}/objects")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteObject(
        string bucketName, [FromQuery] string key, CancellationToken cancellationToken)
    {
        LogHandlingDeleteObject(bucketName, key);
        var result = await _sender.Send(new DeleteS3ObjectCommand(bucketName, key), cancellationToken);
        LogDeleteObjectHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Reads the system properties, user metadata and tags recorded against a single object.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="key">The full key of the object within the bucket.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the object metadata and tags.</returns>
    [HttpGet("buckets/{bucketName}/objects/metadata")]
    [ProducesResponseType(typeof(S3ObjectMetadataResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetObjectMetadata(
        string bucketName, [FromQuery] string key, CancellationToken cancellationToken)
    {
        LogHandlingMetadata(bucketName, key);
        var result = await _sender.Send(new GetS3ObjectMetadataQuery(bucketName, key), cancellationToken);
        LogMetadataHandled(result.IsSuccess);
        return result.Match(
            metadata => Results.Ok(new S3ObjectMetadataResponse(
                metadata.ContentType,
                metadata.ContentLength,
                metadata.LastModified,
                metadata.ETag,
                metadata.Metadata
                    .Select(entry => new S3MetadataEntryResponse(entry.Key, entry.Value))
                    .ToList(),
                metadata.Tags
                    .Select(entry => new S3MetadataEntryResponse(entry.Key, entry.Value))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Replaces the full set of tags recorded against a single object.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="key">The full key of the object within the bucket.</param>
    /// <param name="request">The full set of tags to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("buckets/{bucketName}/objects/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateObjectTags(
        string bucketName,
        [FromQuery] string key,
        [FromBody] S3ObjectTagsUpdateRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingUpdateTags(bucketName, key);
        var result = await _sender.Send(
            new UpdateS3ObjectTagsCommand(bucketName, key, request.Tags), cancellationToken);
        LogUpdateTagsHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Copies a single object to a destination key, optionally in another bucket.
    /// </summary>
    /// <param name="bucketName">The bucket the source object lives in.</param>
    /// <param name="key">The full key of the source object within the bucket.</param>
    /// <param name="request">The destination bucket and key to copy the object to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the prefix the object was copied to.</returns>
    [HttpPost("buckets/{bucketName}/objects/copy")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CopyObject(
        string bucketName,
        [FromQuery] string key,
        [FromBody] S3ObjectCopyRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingCopy(bucketName, key, request.DestinationBucketName, request.DestinationKey);
        var result = await _sender.Send(
            new CopyS3ObjectCommand(bucketName, key, request.DestinationBucketName, request.DestinationKey),
            cancellationToken);
        LogCopyHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(DestinationLocation(request.DestinationBucketName, request.DestinationKey), null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Moves a single object to a destination key, optionally in another bucket, deleting the source.
    /// </summary>
    /// <param name="bucketName">The bucket the source object lives in.</param>
    /// <param name="key">The full key of the source object within the bucket.</param>
    /// <param name="request">The destination bucket and key to move the object to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the prefix the object was moved to.</returns>
    [HttpPost("buckets/{bucketName}/objects/move")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> MoveObject(
        string bucketName,
        [FromQuery] string key,
        [FromBody] S3ObjectCopyRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingMove(bucketName, key, request.DestinationBucketName, request.DestinationKey);
        var result = await _sender.Send(
            new MoveS3ObjectCommand(bucketName, key, request.DestinationBucketName, request.DestinationKey),
            cancellationToken);
        LogMoveHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(DestinationLocation(request.DestinationBucketName, request.DestinationKey), null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Reads the configuration of a single bucket: versioning, default encryption, lifecycle rules,
    /// event notifications and the access policy.
    /// </summary>
    /// <param name="bucketName">The bucket to read the configuration of.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the bucket configuration.</returns>
    [HttpGet("buckets/{bucketName}/configuration")]
    [ProducesResponseType(typeof(S3BucketConfigurationResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetBucketConfiguration(
        string bucketName, CancellationToken cancellationToken)
    {
        LogHandlingConfiguration(bucketName);
        var result = await _sender.Send(new GetS3BucketConfigurationQuery(bucketName), cancellationToken);
        LogConfigurationHandled(result.IsSuccess);
        return result.Match(
            configuration => Results.Ok(new S3BucketConfigurationResponse(
                configuration.VersioningStatus,
                configuration.EncryptionAlgorithm,
                configuration.EncryptionKeyId,
                configuration.LifecycleRules
                    .Select(rule => new S3LifecycleRuleResponse(rule.Id, rule.Status, rule.Prefix))
                    .ToList(),
                configuration.Notifications
                    .Select(notification => new S3NotificationResponse(
                        notification.Type, notification.TargetArn, notification.Events))
                    .ToList(),
                configuration.Policy)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Aggregates a best-effort storage summary for a single bucket: the number of objects stored
    /// and their total size in bytes.
    /// </summary>
    /// <param name="bucketName">The bucket to summarize.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the storage summary.</returns>
    [HttpGet("buckets/{bucketName}/storage-summary")]
    [ProducesResponseType(typeof(S3BucketStorageSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetBucketStorageSummary(
        string bucketName, CancellationToken cancellationToken)
    {
        LogHandlingStorageSummary(bucketName);
        var result = await _sender.Send(new GetS3BucketStorageSummaryQuery(bucketName), cancellationToken);
        LogStorageSummaryHandled(result.IsSuccess);
        return result.Match(
            summary => Results.Ok(new S3BucketStorageSummaryResponse(
                summary.ObjectCount,
                summary.TotalSizeBytes)),
            error => error.AsHttpResult());
    }

    private static string DestinationLocation(string destinationBucketName, string destinationKey)
    {
        var lastSlash = destinationKey.LastIndexOf('/');
        var prefix = lastSlash >= 0 ? destinationKey[..(lastSlash + 1)] : string.Empty;
        return $"/api/services/s3/buckets/{Uri.EscapeDataString(destinationBucketName)}/objects?prefix={Uri.EscapeDataString(prefix)}";
    }

    [LoggerMessage(LogLevel.Trace, "Handling S3 bucket list request.")]
    private partial void LogHandlingList();

    [LoggerMessage(LogLevel.Trace, "S3 bucket list request handled. Success: {Success}")]
    private partial void LogListHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 bucket create request for '{BucketName}'.")]
    private partial void LogHandlingCreate(string bucketName);

    [LoggerMessage(LogLevel.Trace, "S3 bucket create request handled. Success: {Success}")]
    private partial void LogCreateHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 bucket delete request for '{BucketName}'.")]
    private partial void LogHandlingDelete(string bucketName);

    [LoggerMessage(LogLevel.Trace, "S3 bucket delete request handled. Success: {Success}")]
    private partial void LogDeleteHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 object list request for '{BucketName}' under '{Prefix}'.")]
    private partial void LogHandlingListObjects(string bucketName, string prefix);

    [LoggerMessage(LogLevel.Trace, "S3 object list request handled. Success: {Success}")]
    private partial void LogListObjectsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 folder create request for '{BucketName}' key '{FolderKey}'.")]
    private partial void LogHandlingCreateFolder(string bucketName, string folderKey);

    [LoggerMessage(LogLevel.Trace, "S3 folder create request handled. Success: {Success}")]
    private partial void LogCreateFolderHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 object upload request for '{BucketName}' key '{Key}'.")]
    private partial void LogHandlingUpload(string bucketName, string key);

    [LoggerMessage(LogLevel.Trace, "S3 object upload request handled. Success: {Success}")]
    private partial void LogUploadHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 object download request for '{BucketName}' key '{Key}'.")]
    private partial void LogHandlingDownload(string bucketName, string key);

    [LoggerMessage(LogLevel.Trace, "S3 object download request handled. Success: {Success}")]
    private partial void LogDownloadHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 object preview request for '{BucketName}' key '{Key}'.")]
    private partial void LogHandlingPreview(string bucketName, string key);

    [LoggerMessage(LogLevel.Trace, "S3 object preview request handled. Success: {Success}")]
    private partial void LogPreviewHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 object presign request for '{BucketName}' key '{Key}' expiry {ExpirySeconds}s.")]
    private partial void LogHandlingPresign(string bucketName, string key, int expirySeconds);

    [LoggerMessage(LogLevel.Trace, "S3 object presign request handled. Success: {Success}")]
    private partial void LogPresignHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 object delete request for '{BucketName}' key '{Key}'.")]
    private partial void LogHandlingDeleteObject(string bucketName, string key);

    [LoggerMessage(LogLevel.Trace, "S3 object delete request handled. Success: {Success}")]
    private partial void LogDeleteObjectHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 object metadata request for '{BucketName}' key '{Key}'.")]
    private partial void LogHandlingMetadata(string bucketName, string key);

    [LoggerMessage(LogLevel.Trace, "S3 object metadata request handled. Success: {Success}")]
    private partial void LogMetadataHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 object tags update request for '{BucketName}' key '{Key}'.")]
    private partial void LogHandlingUpdateTags(string bucketName, string key);

    [LoggerMessage(LogLevel.Trace, "S3 object tags update request handled. Success: {Success}")]
    private partial void LogUpdateTagsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 object copy request for '{BucketName}' key '{Key}' to '{DestinationBucketName}' key '{DestinationKey}'.")]
    private partial void LogHandlingCopy(string bucketName, string key, string destinationBucketName, string destinationKey);

    [LoggerMessage(LogLevel.Trace, "S3 object copy request handled. Success: {Success}")]
    private partial void LogCopyHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 object move request for '{BucketName}' key '{Key}' to '{DestinationBucketName}' key '{DestinationKey}'.")]
    private partial void LogHandlingMove(string bucketName, string key, string destinationBucketName, string destinationKey);

    [LoggerMessage(LogLevel.Trace, "S3 object move request handled. Success: {Success}")]
    private partial void LogMoveHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 bucket configuration request for '{BucketName}'.")]
    private partial void LogHandlingConfiguration(string bucketName);

    [LoggerMessage(LogLevel.Trace, "S3 bucket configuration request handled. Success: {Success}")]
    private partial void LogConfigurationHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling S3 bucket storage summary request for '{BucketName}'.")]
    private partial void LogHandlingStorageSummary(string bucketName);

    [LoggerMessage(LogLevel.Trace, "S3 bucket storage summary request handled. Success: {Success}")]
    private partial void LogStorageSummaryHandled(bool success);
}
