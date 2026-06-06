using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateRestResource;

/// <summary>
/// Create a child resource under an existing resource of an API Gateway REST API.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the resource belongs to.</param>
/// <param name="ParentId">The identifier of the parent resource the new resource hangs from.</param>
/// <param name="PathPart">The last path segment of the resource to create, for example <c>items</c>.</param>
public record CreateRestResourceCommand(
    string RestApiId,
    string ParentId,
    string PathPart) : ICommand<string>;
