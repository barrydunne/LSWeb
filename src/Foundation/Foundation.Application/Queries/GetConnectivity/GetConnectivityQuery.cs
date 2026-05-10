using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Connectivity;

namespace Foundation.Application.Queries.GetConnectivity;

/// <summary>
/// Query the reachability of the configured AWS backend.
/// </summary>
public record GetConnectivityQuery : IQuery<GetConnectivityQueryResult>;

/// <summary>
/// The result of a connectivity query.
/// </summary>
/// <param name="Connection">The resolved connectivity state.</param>
public record GetConnectivityQueryResult(ConnectionState Connection);
