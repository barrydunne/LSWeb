using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.ResolveReference;

/// <summary>
/// Resolve an ARN or bare resource identifier to the console route for the target resource.
/// </summary>
/// <param name="Reference">The ARN or resource identifier to resolve.</param>
/// <param name="Service">An optional service hint used when the reference is a bare identifier rather than an ARN.</param>
public record ResolveReferenceQuery(string Reference, string? Service) : IQuery<ResolveReferenceQueryResult>;

/// <summary>
/// The resolved console location for a reference.
/// </summary>
/// <param name="ServiceKey">The catalogue key of the owning service.</param>
/// <param name="ResourceId">The bare resource identifier extracted from the reference.</param>
/// <param name="Route">The console route that displays the referenced resource.</param>
public record ResolveReferenceQueryResult(string ServiceKey, string ResourceId, string Route);
