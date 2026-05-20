namespace Foundation.Domain.S3;

/// <summary>
/// A concise view of an S3 bucket as it appears in a bucket list.
/// </summary>
/// <param name="Name">The globally unique name of the bucket.</param>
/// <param name="CreationDate">The timestamp the bucket was created, as reported by AWS; empty when not reported.</param>
public sealed record S3Bucket(
    string Name,
    string CreationDate);
