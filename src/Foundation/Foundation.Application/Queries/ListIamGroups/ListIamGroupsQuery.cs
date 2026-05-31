using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Iam;

namespace Foundation.Application.Queries.ListIamGroups;

/// <summary>
/// List the IAM groups available on the backend.
/// </summary>
public record ListIamGroupsQuery : IQuery<ListIamGroupsQueryResult>;

/// <summary>
/// The IAM groups available on the backend.
/// </summary>
/// <param name="Groups">The groups, ordered as returned by the backend.</param>
public record ListIamGroupsQueryResult(IReadOnlyList<IamGroup> Groups);
