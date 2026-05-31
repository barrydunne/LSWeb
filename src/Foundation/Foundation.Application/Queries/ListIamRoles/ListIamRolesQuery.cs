using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Iam;

namespace Foundation.Application.Queries.ListIamRoles;

/// <summary>
/// List the IAM roles available on the backend.
/// </summary>
public record ListIamRolesQuery : IQuery<ListIamRolesQueryResult>;

/// <summary>
/// The IAM roles available on the backend.
/// </summary>
/// <param name="Roles">The roles, ordered as returned by the backend.</param>
public record ListIamRolesQueryResult(IReadOnlyList<IamRole> Roles);
