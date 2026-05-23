using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateS3ObjectTags;

/// <summary>
/// Replace the full set of tags recorded against a single S3 object.
/// </summary>
/// <param name="BucketName">The bucket the object lives in.</param>
/// <param name="Key">The full key of the object within the bucket.</param>
/// <param name="Tags">The full set of tags to apply, keyed by tag name.</param>
public record UpdateS3ObjectTagsCommand(
    string BucketName, string Key, IReadOnlyDictionary<string, string> Tags) : ICommand;
