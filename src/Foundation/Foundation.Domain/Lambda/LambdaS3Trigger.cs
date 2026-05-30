namespace Foundation.Domain.Lambda;

/// <summary>
/// An S3 bucket configured to invoke a Lambda function. Unlike an event source mapping, an S3
/// trigger is defined on the bucket's notification configuration. It is discovered both from the
/// bucket's notification configuration (the authoritative source) and from the function's
/// resource-based policy, where the S3 service is granted permission to invoke the function for a
/// specific source bucket.
/// </summary>
/// <param name="BucketArn">The Amazon Resource Name of the source bucket.</param>
public sealed record LambdaS3Trigger(string BucketArn);
