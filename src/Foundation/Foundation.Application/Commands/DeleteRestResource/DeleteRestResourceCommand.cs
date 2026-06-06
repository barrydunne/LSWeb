using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteRestResource;

/// <summary>
/// Delete a resource from an API Gateway REST API. This action cannot be undone.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the resource belongs to.</param>
/// <param name="ResourceId">The identifier of the resource to delete.</param>
public record DeleteRestResourceCommand(string RestApiId, string ResourceId) : ICommand;
