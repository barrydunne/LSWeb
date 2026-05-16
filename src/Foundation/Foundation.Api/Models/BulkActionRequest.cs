namespace Foundation.Api.Models;

/// <summary>
/// A request to apply a bulk action to a set of resources.
/// </summary>
/// <param name="ResourceIds">The identifiers of the resources to apply the action to.</param>
public sealed record BulkActionRequest(IReadOnlyList<string> ResourceIds);
