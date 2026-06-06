using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Application.Queries.GetHttpApi;

/// <summary>
/// Reads the full configuration of a single Amazon API Gateway v2 API.
/// </summary>
/// <param name="ApiId">The unique identifier of the API to read.</param>
public record GetHttpApiQuery(string ApiId) : IQuery<GetHttpApiQueryResult>;

/// <summary>
/// The result of reading an API.
/// </summary>
/// <param name="Api">The API detail.</param>
public record GetHttpApiQueryResult(HttpApiDetail Api);
