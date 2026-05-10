using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Capabilities;
using Foundation.Domain.Catalogue;

namespace Foundation.Application.Queries.GetCatalogue;

/// <summary>
/// Query the catalogue of managed AWS services.
/// </summary>
public record GetCatalogueQuery : IQuery<GetCatalogueQueryResult>;

/// <summary>
/// The result of a catalogue query.
/// </summary>
/// <param name="Services">The managed services in display order.</param>
/// <param name="Capabilities">The capability snapshot describing which services the backend supports.</param>
public record GetCatalogueQueryResult(IReadOnlyList<ServiceDescriptor> Services, CapabilityMap Capabilities);
