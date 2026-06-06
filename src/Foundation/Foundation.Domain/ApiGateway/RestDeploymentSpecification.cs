namespace Foundation.Domain.ApiGateway;

/// <summary>
/// The desired configuration of an API Gateway REST API deployment to create.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the deployment belongs to.</param>
/// <param name="StageName">An optional stage name to create and point at the new deployment, or <see langword="null"/> to create the deployment only.</param>
/// <param name="Description">An optional human-readable description, or <see langword="null"/> when none is set.</param>
public sealed record RestDeploymentSpecification(
    string RestApiId,
    string? StageName,
    string? Description);
