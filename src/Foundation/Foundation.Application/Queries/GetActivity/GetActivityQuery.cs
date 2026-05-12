using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Activity;

namespace Foundation.Application.Queries.GetActivity;

/// <summary>
/// Query the in-session activity log of completed backend operations.
/// </summary>
public record GetActivityQuery : IQuery<GetActivityQueryResult>;

/// <summary>
/// The result of an activity query.
/// </summary>
/// <param name="Entries">The recorded activity entries, most recent first.</param>
public record GetActivityQueryResult(IReadOnlyList<ActivityEntry> Entries);
