using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGateway;

namespace Foundation.Application.Queries.GetRestMethod;

/// <summary>
/// Read the configuration of a single HTTP method on an API Gateway REST API resource.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API.</param>
/// <param name="ResourceId">The identifier of the resource.</param>
/// <param name="HttpMethod">The HTTP verb of the method to read.</param>
public record GetRestMethodQuery(string RestApiId, string ResourceId, string HttpMethod)
    : IQuery<GetRestMethodQueryResult>;

/// <summary>
/// The result of reading a REST API method.
/// </summary>
/// <param name="Method">The method detail.</param>
public record GetRestMethodQueryResult(RestMethodDetail Method);
