using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGateway;

namespace Foundation.Application.Queries.ListRestApis;

/// <summary>
/// List the API Gateway REST APIs available on the backend.
/// </summary>
public record ListRestApisQuery : IQuery<ListRestApisQueryResult>;

/// <summary>
/// The API Gateway REST APIs available on the backend.
/// </summary>
/// <param name="RestApis">The REST APIs, ordered as returned by the backend.</param>
public record ListRestApisQueryResult(IReadOnlyList<RestApi> RestApis);
