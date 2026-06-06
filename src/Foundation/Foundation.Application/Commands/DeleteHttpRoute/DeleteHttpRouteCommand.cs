using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteHttpRoute;

/// <summary>
/// Delete an Amazon API Gateway v2 route. This action cannot be undone.
/// </summary>
/// <param name="ApiId">The identifier of the API the route belongs to.</param>
/// <param name="RouteId">The unique identifier of the route to delete.</param>
public record DeleteHttpRouteCommand(string ApiId, string RouteId) : ICommand;
