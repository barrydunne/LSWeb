using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Ses;

namespace Foundation.Application.Queries.GetSesIdentity;

/// <summary>
/// Get the verification detail for a single SES identity.
/// </summary>
/// <param name="Identity">The email address or domain name to look up.</param>
public record GetSesIdentityQuery(string Identity) : IQuery<GetSesIdentityQueryResult>;

/// <summary>
/// The verification detail for a single SES identity.
/// </summary>
/// <param name="Identity">The identity detail.</param>
public record GetSesIdentityQueryResult(SesIdentityDetail Identity);
