namespace Foundation.Api.Models;

/// <summary>
/// The resolved console location for an ARN or resource identifier.
/// </summary>
/// <param name="ServiceKey">The catalogue key of the owning service.</param>
/// <param name="ResourceId">The bare resource identifier extracted from the reference.</param>
/// <param name="Route">The console route that displays the referenced resource.</param>
public sealed record ResolveReferenceResponse(string ServiceKey, string ResourceId, string Route);
