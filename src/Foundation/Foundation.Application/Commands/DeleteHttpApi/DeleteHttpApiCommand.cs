using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteHttpApi;

/// <summary>
/// Delete an Amazon API Gateway v2 API. This action cannot be undone.
/// </summary>
/// <param name="ApiId">The unique identifier of the API to delete.</param>
public record DeleteHttpApiCommand(string ApiId) : ICommand;
