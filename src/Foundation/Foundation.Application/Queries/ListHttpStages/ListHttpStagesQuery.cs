using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Application.Queries.ListHttpStages;

/// <summary>
/// Lists the stages of an Amazon API Gateway v2 API.
/// </summary>
/// <param name="ApiId">The unique identifier of the API whose stages to list.</param>
public record ListHttpStagesQuery(string ApiId) : IQuery<ListHttpStagesQueryResult>;

/// <summary>
/// The result of listing stages.
/// </summary>
/// <param name="Stages">The stages found on the API.</param>
public record ListHttpStagesQueryResult(IReadOnlyList<HttpStageSummary> Stages);
