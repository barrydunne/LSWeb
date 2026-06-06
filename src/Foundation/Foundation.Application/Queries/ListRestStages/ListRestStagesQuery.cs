using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGateway;

namespace Foundation.Application.Queries.ListRestStages;

/// <summary>
/// List the stages configured on an API Gateway REST API.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API whose stages to list.</param>
public record ListRestStagesQuery(string RestApiId) : IQuery<ListRestStagesQueryResult>;

/// <summary>
/// The result of listing the stages of a REST API.
/// </summary>
/// <param name="Stages">The stages of the REST API.</param>
public record ListRestStagesQueryResult(IReadOnlyList<RestStageSummary> Stages);
