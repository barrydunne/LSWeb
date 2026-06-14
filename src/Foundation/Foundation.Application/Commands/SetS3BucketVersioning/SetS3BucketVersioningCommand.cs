using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.SetS3BucketVersioning;

/// <summary>
/// Enable or suspend versioning on an S3 bucket.
/// </summary>
/// <param name="BucketName">The bucket to update.</param>
/// <param name="Enabled">When <see langword="true"/> versioning is enabled; otherwise it is suspended.</param>
public record SetS3BucketVersioningCommand(string BucketName, bool Enabled) : ICommand;
