namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// A concise view of an Amazon API Gateway v2 stage as it appears in a list.
/// </summary>
/// <param name="StageName">The name of the stage (for example <c>$default</c>, <c>dev</c> or <c>prod</c>).</param>
/// <param name="AutoDeploy">Whether updates to the API are automatically deployed to the stage.</param>
/// <param name="DeploymentId">The identifier of the deployment currently associated with the stage, or <see langword="null"/> when none.</param>
/// <param name="CreatedDate">The time the stage was created, or <see langword="null"/> when unknown.</param>
public sealed record HttpStageSummary(
    string StageName,
    bool AutoDeploy,
    string? DeploymentId,
    DateTimeOffset? CreatedDate);
