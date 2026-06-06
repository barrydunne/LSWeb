namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// The desired configuration of an Amazon API Gateway v2 stage when creating or updating one.
/// </summary>
/// <param name="ApiId">The identifier of the API the stage belongs to.</param>
/// <param name="StageName">The name of the stage (for example <c>$default</c>, <c>dev</c> or <c>prod</c>).</param>
/// <param name="AutoDeploy">Whether updates to the API are automatically deployed to the stage.</param>
/// <param name="Description">A human-readable description of the stage, or <see langword="null"/> when none.</param>
/// <param name="DefaultRouteThrottlingBurstLimit">The default route throttling burst limit, or <see langword="null"/> when not configured.</param>
/// <param name="DefaultRouteThrottlingRateLimit">The default route throttling rate limit, or <see langword="null"/> when not configured.</param>
/// <param name="StageVariables">The stage variables to configure on the stage.</param>
public sealed record HttpStageSpecification(
    string ApiId,
    string StageName,
    bool AutoDeploy,
    string? Description,
    int? DefaultRouteThrottlingBurstLimit,
    double? DefaultRouteThrottlingRateLimit,
    IReadOnlyDictionary<string, string> StageVariables);
