using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Cognito;

namespace Foundation.Application.Queries.ListUserPools;

/// <summary>
/// Lists the Amazon Cognito user pools available on the backend.
/// </summary>
public record ListUserPoolsQuery : IQuery<ListUserPoolsQueryResult>;

/// <summary>
/// The result of listing user pools.
/// </summary>
/// <param name="UserPools">The user pools found on the backend.</param>
public record ListUserPoolsQueryResult(IReadOnlyList<UserPoolSummary> UserPools);
