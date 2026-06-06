using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Application.Queries.ListHttpApis;

/// <summary>
/// Lists the Amazon API Gateway v2 APIs available on the backend.
/// </summary>
public record ListHttpApisQuery() : IQuery<ListHttpApisQueryResult>;

/// <summary>
/// The result of listing APIs.
/// </summary>
/// <param name="Apis">The APIs found on the backend.</param>
public record ListHttpApisQueryResult(IReadOnlyList<HttpApiSummary> Apis);
