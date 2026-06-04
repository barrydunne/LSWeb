using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Ses;

namespace Foundation.Application.Queries.ListSesIdentities;

/// <summary>
/// List the SES identities available on the backend.
/// </summary>
public record ListSesIdentitiesQuery : IQuery<ListSesIdentitiesQueryResult>;

/// <summary>
/// The SES identities available on the backend.
/// </summary>
/// <param name="Identities">The identities, ordered as returned by the backend.</param>
public record ListSesIdentitiesQueryResult(IReadOnlyList<SesIdentity> Identities);
