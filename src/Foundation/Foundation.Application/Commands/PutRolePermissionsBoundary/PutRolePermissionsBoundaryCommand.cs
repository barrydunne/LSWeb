using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.PutRolePermissionsBoundary;

/// <summary>
/// Set the permissions boundary of an IAM role to a managed policy.
/// </summary>
/// <param name="RoleName">The name of the role to set the boundary on.</param>
/// <param name="PermissionsBoundaryArn">The ARN of the managed policy to use as the boundary.</param>
public record PutRolePermissionsBoundaryCommand(string RoleName, string PermissionsBoundaryArn) : ICommand;
