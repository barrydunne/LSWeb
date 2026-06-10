using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGateway;

namespace Foundation.Application.Queries.GetRestCors;

/// <summary>
/// Read the CORS policy configured on a single resource of an API Gateway REST API.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API.</param>
/// <param name="ResourceId">The identifier of the resource whose CORS policy to read.</param>
public record GetRestCorsQuery(string RestApiId, string ResourceId)
    : IQuery<GetRestCorsQueryResult>;

/// <summary>
/// The result of reading a REST API resource CORS policy.
/// </summary>
/// <param name="Cors">The CORS configuration of the resource.</param>
public record GetRestCorsQueryResult(RestCorsConfiguration Cors);
