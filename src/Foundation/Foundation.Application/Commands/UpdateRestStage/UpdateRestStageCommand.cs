using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateRestStage;

/// <summary>
/// Update the configuration of an existing API Gateway REST API stage.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the stage belongs to.</param>
/// <param name="StageName">The name of the stage to update.</param>
/// <param name="Description">An optional human-readable description.</param>
/// <param name="Variables">The stage variables keyed by name.</param>
public record UpdateRestStageCommand(
    string RestApiId,
    string StageName,
    string? Description,
    IReadOnlyDictionary<string, string> Variables) : ICommand;
