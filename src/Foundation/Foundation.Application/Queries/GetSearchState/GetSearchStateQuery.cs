using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.GetSearchState;

/// <summary>
/// Query the current state of the background search index, such as when it was last built and how
/// many entries it holds.
/// </summary>
public record GetSearchStateQuery : IQuery<GetSearchStateQueryResult>;

/// <summary>
/// The state of the background search index.
/// </summary>
/// <param name="BuiltAt">When the current snapshot was built.</param>
/// <param name="EntryCount">The number of entries in the current snapshot.</param>
/// <param name="IsBuilding">Whether a rebuild is currently in progress.</param>
public record GetSearchStateQueryResult(DateTimeOffset BuiltAt, int EntryCount, bool IsBuilding);
