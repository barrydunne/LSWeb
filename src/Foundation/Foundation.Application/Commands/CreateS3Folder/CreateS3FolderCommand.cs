using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateS3Folder;

/// <summary>
/// Create a zero-byte folder marker so an empty prefix appears as a navigable folder.
/// </summary>
/// <param name="BucketName">The bucket the folder lives in.</param>
/// <param name="FolderKey">The full folder key, ending with a delimiter.</param>
public record CreateS3FolderCommand(string BucketName, string FolderKey) : ICommand;
