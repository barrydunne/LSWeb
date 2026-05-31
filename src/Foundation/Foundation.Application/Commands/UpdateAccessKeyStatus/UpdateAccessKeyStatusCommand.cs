using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateAccessKeyStatus;

/// <summary>
/// Update the status of an IAM user's access key.
/// </summary>
/// <param name="UserName">The name of the user that owns the access key.</param>
/// <param name="AccessKeyId">The identifier of the access key to update.</param>
/// <param name="Status">The new status, either <c>Active</c> or <c>Inactive</c>.</param>
public record UpdateAccessKeyStatusCommand(string UserName, string AccessKeyId, string Status) : ICommand;
