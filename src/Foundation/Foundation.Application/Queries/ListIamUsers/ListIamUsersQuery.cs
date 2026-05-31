using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Iam;

namespace Foundation.Application.Queries.ListIamUsers;

/// <summary>
/// List the IAM users available on the backend.
/// </summary>
public record ListIamUsersQuery : IQuery<ListIamUsersQueryResult>;

/// <summary>
/// The IAM users available on the backend.
/// </summary>
/// <param name="Users">The users, ordered as returned by the backend.</param>
public record ListIamUsersQueryResult(IReadOnlyList<IamUser> Users);
