using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Application.Queries.GetHttpStage;

/// <summary>
/// Reads the full configuration of a single Amazon API Gateway v2 stage.
/// </summary>
/// <param name="ApiId">The unique identifier of the API the stage belongs to.</param>
/// <param name="StageName">The name of the stage to read.</param>
public record GetHttpStageQuery(string ApiId, string StageName) : IQuery<GetHttpStageQueryResult>;

/// <summary>
/// The result of reading a stage.
/// </summary>
/// <param name="Stage">The stage detail.</param>
public record GetHttpStageQueryResult(HttpStageDetail Stage);
