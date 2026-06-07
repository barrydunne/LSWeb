using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Seed;

namespace Foundation.Application.Queries.GetSeedTemplates;

/// <summary>
/// Query the catalogue of available seed templates.
/// </summary>
public record GetSeedTemplatesQuery : IQuery<GetSeedTemplatesQueryResult>;

/// <summary>
/// The result of a seed templates query.
/// </summary>
/// <param name="Templates">The available seed templates in display order.</param>
public record GetSeedTemplatesQueryResult(IReadOnlyList<SeedTemplate> Templates);
