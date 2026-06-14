using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Ses;

namespace Foundation.Application.Queries.GetSesDomainSetup;

/// <summary>
/// Get the domain verification and DKIM setup state for an SES domain identity.
/// </summary>
/// <param name="Domain">The domain name to look up.</param>
public record GetSesDomainSetupQuery(string Domain) : IQuery<GetSesDomainSetupQueryResult>;

/// <summary>
/// The domain verification and DKIM setup state for an SES domain identity.
/// </summary>
/// <param name="Setup">The domain setup state.</param>
public record GetSesDomainSetupQueryResult(SesDomainSetup Setup);
