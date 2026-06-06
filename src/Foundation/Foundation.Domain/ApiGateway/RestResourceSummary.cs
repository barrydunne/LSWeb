namespace Foundation.Domain.ApiGateway;

/// <summary>
/// A node in the resource tree of an API Gateway REST API.
/// </summary>
/// <param name="Id">The identifier that uniquely identifies the resource.</param>
/// <param name="ParentId">The identifier of the parent resource, or <see langword="null"/> for the root resource.</param>
/// <param name="PathPart">The last path segment of the resource, or <see langword="null"/> for the root resource.</param>
/// <param name="Path">The full path of the resource from the root, for example <c>/items/{id}</c>.</param>
/// <param name="ResourceMethods">The HTTP methods configured on the resource (for example <c>GET</c>, <c>POST</c>).</param>
public sealed record RestResourceSummary(
    string Id,
    string? ParentId,
    string? PathPart,
    string Path,
    IReadOnlyList<string> ResourceMethods);
