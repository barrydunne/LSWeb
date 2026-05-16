using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.GetFavourites;

/// <summary>
/// Query the user's pinned favourite resource references.
/// </summary>
public record GetFavouritesQuery : IQuery<GetFavouritesQueryResult>;

/// <summary>
/// The result of a favourites query.
/// </summary>
/// <param name="References">The pinned favourite resource references, in the order they were pinned.</param>
public record GetFavouritesQueryResult(IReadOnlyList<string> References);
