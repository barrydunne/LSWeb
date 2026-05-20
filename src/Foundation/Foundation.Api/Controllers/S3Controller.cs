using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateS3Bucket;
using Foundation.Application.Commands.DeleteS3Bucket;
using Foundation.Application.Queries.ListS3Buckets;
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
}
