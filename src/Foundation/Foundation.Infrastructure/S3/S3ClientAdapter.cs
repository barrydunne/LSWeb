using System.Diagnostics.CodeAnalysis;
using Amazon.S3;
using Amazon.S3.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.S3;
using Foundation.Infrastructure.Aws;
using S3Bucket = Foundation.Domain.S3.S3Bucket;

namespace Foundation.Infrastructure.S3;

/// <summary>
/// Reads and manages S3 buckets through the resilient AWS gateway so the same code works against
/// LocalStack or real AWS. All access flows through <see cref="IAwsGateway"/>, which records
/// capability and converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class S3ClientAdapter : IS3Client
{
    private const string ServiceKey = "s3";

    private readonly IAwsGateway _gateway;

    public S3ClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<S3Bucket>>> ListBucketsAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonS3Client, IReadOnlyList<S3Bucket>>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.ListBucketsAsync(new ListBucketsRequest(), token);
                return (response.Buckets ?? [])
                    .Select(S3BucketMapper.ToBucket)
                    .ToList();
            },
            cancellationToken);

    public async Task<Result> CreateBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutBucketAsync(new PutBucketRequest { BucketName = bucketName }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonS3Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteBucketAsync(new DeleteBucketRequest { BucketName = bucketName }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }
}
