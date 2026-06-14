using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteS3ObjectVersion;

/// <summary>
/// Delete a specific version of an S3 object.
/// </summary>
/// <param name="BucketName">The bucket containing the object.</param>
/// <param name="Key">The object key.</param>
/// <param name="VersionId">The version identifier to delete.</param>
public record DeleteS3ObjectVersionCommand(string BucketName, string Key, string VersionId) : ICommand;
