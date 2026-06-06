namespace Foundation.Domain.ApiGateway;

/// <summary>
/// The desired configuration of an API Gateway REST API stage to create or update.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the stage belongs to.</param>
/// <param name="StageName">The name of the stage, for example <c>dev</c> or <c>prod</c>.</param>
/// <param name="DeploymentId">The identifier of the deployment the stage points at. Required when creating.</param>
/// <param name="Description">An optional human-readable description, or <see langword="null"/> when none is set.</param>
/// <param name="Variables">The stage variables keyed by name.</param>
public sealed record RestStageSpecification(
    string RestApiId,
    string StageName,
    string DeploymentId,
    string? Description,
    IReadOnlyDictionary<string, string> Variables);
