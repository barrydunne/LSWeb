using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Application.Queries.GetHttpAuthorizer;

/// <summary>
/// Reads the full configuration of a single Amazon API Gateway v2 authorizer.
/// </summary>
/// <param name="ApiId">The unique identifier of the API the authorizer belongs to.</param>
/// <param name="AuthorizerId">The unique identifier of the authorizer to read.</param>
public record GetHttpAuthorizerQuery(string ApiId, string AuthorizerId) : IQuery<GetHttpAuthorizerQueryResult>;

/// <summary>
/// The result of reading an authorizer.
/// </summary>
/// <param name="Authorizer">The authorizer detail.</param>
public record GetHttpAuthorizerQueryResult(HttpAuthorizerDetail Authorizer);
