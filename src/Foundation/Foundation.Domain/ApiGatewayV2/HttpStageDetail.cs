namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// The full configuration of an Amazon API Gateway v2 stage.
/// </summary>
/// <param name="StageName">The name of the stage (for example <c>$default</c>, <c>dev</c> or <c>prod</c>).</param>
/// <param name="AutoDeploy">Whether updates to the API are automatically deployed to the stage.</param>
/// <param name="DeploymentId">The identifier of the deployment currently associated with the stage, or <see langword="null"/> when none.</param>
/// <param name="Description">A human-readable description of the stage, or <see langword="null"/> when none.</param>
/// <param name="DefaultRouteThrottlingBurstLimit">The default route throttling burst limit, or <see langword="null"/> when not configured.</param>
/// <param name="DefaultRouteThrottlingRateLimit">The default route throttling rate limit, or <see langword="null"/> when not configured.</param>
/// <param name="StageVariables">The stage variables configured for the stage.</param>
/// <param name="CreatedDate">The time the stage was created, or <see langword="null"/> when unknown.</param>
/// <param name="LastUpdatedDate">The time the stage was last updated, or <see langword="null"/> when unknown.</param>
public sealed record HttpStageDetail(
    string StageName,
    bool AutoDeploy,
    string? DeploymentId,
    string? Description,
    int? DefaultRouteThrottlingBurstLimit,
    double? DefaultRouteThrottlingRateLimit,
    IReadOnlyDictionary<string, string> StageVariables,
    DateTimeOffset? CreatedDate,
    DateTimeOffset? LastUpdatedDate);
