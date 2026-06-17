using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Iam;

namespace Foundation.Application.Queries.GetIamRole;

/// <summary>
/// Get the full detail of a single IAM role.
/// </summary>
/// <param name="RoleName">The name of the role to retrieve.</param>
public record GetIamRoleQuery(string RoleName) : IQuery<GetIamRoleQueryResult>;

/// <summary>
/// The full detail of a single IAM role.
/// </summary>
/// <param name="Role">The role detail, or <c>null</c> when no such role exists.</param>
public record GetIamRoleQueryResult(IamRoleDetail? Role);
