using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteAccessKey;

/// <summary>
/// Delete an IAM user's access key.
/// </summary>
/// <param name="UserName">The name of the user that owns the access key.</param>
/// <param name="AccessKeyId">The identifier of the access key to delete.</param>
public record DeleteAccessKeyCommand(string UserName, string AccessKeyId) : ICommand;
