using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGateway;

namespace Foundation.Application.Queries.GetRestStage;

/// <summary>
/// Read the configuration of a single stage on an API Gateway REST API.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API.</param>
/// <param name="StageName">The name of the stage to read.</param>
public record GetRestStageQuery(string RestApiId, string StageName)
    : IQuery<GetRestStageQueryResult>;

/// <summary>
/// The result of reading a REST API stage.
/// </summary>
/// <param name="Stage">The stage detail.</param>
public record GetRestStageQueryResult(RestStageDetail Stage);
