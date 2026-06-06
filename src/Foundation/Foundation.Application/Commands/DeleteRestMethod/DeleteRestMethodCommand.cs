using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteRestMethod;

/// <summary>
/// Delete an HTTP method from an API Gateway REST API resource. This action cannot be undone.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the method belongs to.</param>
/// <param name="ResourceId">The identifier of the resource the method belongs to.</param>
/// <param name="HttpMethod">The HTTP verb of the method to delete.</param>
public record DeleteRestMethodCommand(
    string RestApiId,
    string ResourceId,
    string HttpMethod) : ICommand;
