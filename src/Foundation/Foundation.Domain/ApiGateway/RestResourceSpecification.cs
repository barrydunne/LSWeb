namespace Foundation.Domain.ApiGateway;

/// <summary>
/// The desired configuration of an API Gateway REST API resource when creating one.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the resource belongs to.</param>
/// <param name="ParentId">The identifier of the parent resource the new resource hangs from.</param>
/// <param name="PathPart">The last path segment of the resource to create, for example <c>items</c>.</param>
public sealed record RestResourceSpecification(
    string RestApiId,
    string ParentId,
    string PathPart);
