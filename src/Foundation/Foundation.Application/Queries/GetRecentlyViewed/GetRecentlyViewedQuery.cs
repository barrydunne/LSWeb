using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.GetRecentlyViewed;

/// <summary>
/// Query the user's recently-viewed resource references, most recent first.
/// </summary>
public record GetRecentlyViewedQuery : IQuery<GetRecentlyViewedQueryResult>;

/// <summary>
/// The result of a recently-viewed query.
/// </summary>
/// <param name="References">The recently-viewed resource references, most recent first.</param>
public record GetRecentlyViewedQueryResult(IReadOnlyList<string> References);
