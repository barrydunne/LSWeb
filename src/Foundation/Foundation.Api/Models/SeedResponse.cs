namespace Foundation.Api.Models;

/// <summary>
/// A seed template available for one-click resource provisioning.
/// </summary>
/// <param name="Id">The stable identifier used to apply the template.</param>
/// <param name="Name">The human-readable name shown on the seed button.</param>
/// <param name="Description">A short summary of what the template provisions.</param>
/// <param name="Resources">The resources the template creates, in the order they are provisioned.</param>
public sealed record SeedTemplateResponse(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<SeedResourceResponse> Resources);

/// <summary>
/// A single resource that a seed template provisions.
/// </summary>
/// <param name="ServiceKey">The catalogue key of the owning service, for example <c>sqs</c>.</param>
/// <param name="ResourceType">The human-readable resource type, for example <c>Queue</c>.</param>
/// <param name="Name">The name the resource will be created with.</param>
public sealed record SeedResourceResponse(string ServiceKey, string ResourceType, string Name);

/// <summary>
/// The catalogue of available seed templates.
/// </summary>
/// <param name="Templates">The available seed templates in display order.</param>
public sealed record SeedTemplatesResponse(IReadOnlyList<SeedTemplateResponse> Templates);

/// <summary>
/// The aggregate outcome of applying a seed template, including per-resource results so that partial
/// success is visible.
/// </summary>
/// <param name="OperationId">The identifier correlating this outcome with its lifecycle notifications.</param>
/// <param name="TemplateId">The identifier of the template that was applied.</param>
/// <param name="TotalCount">The total number of resources the template attempted to create.</param>
/// <param name="SucceededCount">The number of resources that were created successfully.</param>
/// <param name="FailedCount">The number of resources that failed to be created.</param>
/// <param name="OverallState">The overall terminal state, either <c>Succeeded</c> or <c>Failed</c>.</param>
/// <param name="Items">The per-resource results in the order the resources were provisioned.</param>
public sealed record SeedOutcomeResponse(
    string OperationId,
    string TemplateId,
    int TotalCount,
    int SucceededCount,
    int FailedCount,
    string OverallState,
    IReadOnlyList<SeedResourceResultResponse> Items);

/// <summary>
/// The result of provisioning a single resource from a seed template.
/// </summary>
/// <param name="ServiceKey">The catalogue key of the owning service, for example <c>sqs</c>.</param>
/// <param name="ResourceType">The human-readable resource type, for example <c>Queue</c>.</param>
/// <param name="Name">The name the resource was created with.</param>
/// <param name="Succeeded">Whether the resource was created successfully.</param>
/// <param name="Error">A human-readable failure reason when the resource was not created; otherwise <see langword="null"/>.</param>
public sealed record SeedResourceResultResponse(
    string ServiceKey,
    string ResourceType,
    string Name,
    bool Succeeded,
    string? Error);
