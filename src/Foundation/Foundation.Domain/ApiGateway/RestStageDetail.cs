namespace Foundation.Domain.ApiGateway;

/// <summary>
/// A detailed view of an API Gateway REST API stage.
/// </summary>
/// <param name="StageName">The name of the stage, for example <c>dev</c> or <c>prod</c>.</param>
/// <param name="DeploymentId">The identifier of the deployment the stage points at.</param>
/// <param name="Description">An optional human-readable description, or <see langword="null"/> when none is set.</param>
/// <param name="CacheClusterEnabled">Whether a cache cluster is enabled for the stage.</param>
/// <param name="Variables">The stage variables keyed by name.</param>
/// <param name="CreatedDate">The UTC creation timestamp, or <see langword="null"/> when not reported.</param>
/// <param name="LastUpdatedDate">The UTC last-updated timestamp, or <see langword="null"/> when not reported.</param>
public sealed record RestStageDetail(
    string StageName,
    string DeploymentId,
    string? Description,
    bool CacheClusterEnabled,
    IReadOnlyDictionary<string, string> Variables,
    DateTimeOffset? CreatedDate,
    DateTimeOffset? LastUpdatedDate);
