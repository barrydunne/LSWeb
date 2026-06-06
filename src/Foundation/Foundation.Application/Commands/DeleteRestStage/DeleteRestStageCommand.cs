using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteRestStage;

/// <summary>
/// Delete a stage from an API Gateway REST API.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the stage belongs to.</param>
/// <param name="StageName">The name of the stage to delete.</param>
public record DeleteRestStageCommand(string RestApiId, string StageName) : ICommand;
