using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateRestStage;

/// <summary>
/// Create a stage on an API Gateway REST API that points at an existing deployment.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the stage belongs to.</param>
/// <param name="StageName">The name of the stage to create.</param>
/// <param name="DeploymentId">The identifier of the deployment the stage points at.</param>
/// <param name="Description">An optional human-readable description.</param>
/// <param name="Variables">The stage variables keyed by name.</param>
public record CreateRestStageCommand(
    string RestApiId,
    string StageName,
    string DeploymentId,
    string? Description,
    IReadOnlyDictionary<string, string> Variables) : ICommand<string>;
