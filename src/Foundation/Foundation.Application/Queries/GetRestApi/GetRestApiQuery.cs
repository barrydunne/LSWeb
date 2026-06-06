using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGateway;

namespace Foundation.Application.Queries.GetRestApi;

/// <summary>
/// Read the full configuration of a single API Gateway REST API.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API to read.</param>
public record GetRestApiQuery(string RestApiId) : IQuery<GetRestApiQueryResult>;

/// <summary>
/// The result of reading a REST API.
/// </summary>
/// <param name="RestApi">The REST API detail.</param>
public record GetRestApiQueryResult(RestApiDetail RestApi);
