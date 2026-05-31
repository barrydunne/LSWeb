using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.PutUserPermissionsBoundary;

/// <summary>
/// Set the permissions boundary of an IAM user to a managed policy.
/// </summary>
/// <param name="UserName">The name of the user to set the boundary on.</param>
/// <param name="PermissionsBoundaryArn">The ARN of the managed policy to use as the boundary.</param>
public record PutUserPermissionsBoundaryCommand(string UserName, string PermissionsBoundaryArn) : ICommand;
