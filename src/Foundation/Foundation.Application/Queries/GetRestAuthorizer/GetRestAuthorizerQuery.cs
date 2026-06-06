using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGateway;

namespace Foundation.Application.Queries.GetRestAuthorizer;

/// <summary>
/// Read the configuration of a single authorizer on an API Gateway REST API.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API.</param>
/// <param name="AuthorizerId">The identifier of the authorizer to read.</param>
public record GetRestAuthorizerQuery(string RestApiId, string AuthorizerId)
    : IQuery<GetRestAuthorizerQueryResult>;

/// <summary>
/// The result of reading a REST API authorizer.
/// </summary>
/// <param name="Authorizer">The authorizer detail.</param>
public record GetRestAuthorizerQueryResult(RestAuthorizerDetail Authorizer);
