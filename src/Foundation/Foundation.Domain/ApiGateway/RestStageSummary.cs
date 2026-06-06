namespace Foundation.Domain.ApiGateway;

/// <summary>
/// A concise view of an API Gateway REST API stage as it appears in a list.
/// </summary>
/// <param name="StageName">The name of the stage, for example <c>dev</c> or <c>prod</c>.</param>
/// <param name="DeploymentId">The identifier of the deployment the stage points at.</param>
/// <param name="CreatedDate">The UTC creation timestamp, or <see langword="null"/> when not reported.</param>
public sealed record RestStageSummary(
    string StageName,
    string DeploymentId,
    DateTimeOffset? CreatedDate);
