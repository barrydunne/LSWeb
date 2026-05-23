using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UploadS3Object;

/// <summary>
/// Upload an object to an S3 bucket, streaming the supplied content under the given key.
/// </summary>
/// <param name="BucketName">The bucket the object lives in.</param>
/// <param name="Key">The full key of the object within the bucket.</param>
/// <param name="Content">The content stream to upload.</param>
/// <param name="ContentType">The content type to record for the object.</param>
public record UploadS3ObjectCommand(string BucketName, string Key, Stream Content, string ContentType) : ICommand;
