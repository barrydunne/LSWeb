using Foundation.Domain.Streaming;

namespace Foundation.Domain.Seed;

/// <summary>
/// The aggregate outcome of applying a <see cref="SeedTemplate"/>, carrying the per-resource results
/// so that partial success can be reported to the caller.
/// </summary>
/// <param name="OperationId">The identifier correlating this outcome with its lifecycle notifications.</param>
/// <param name="TemplateId">The identifier of the template that was applied.</param>
/// <param name="Items">The per-resource results in the order the resources were provisioned.</param>
public record SeedOutcome(string OperationId, string TemplateId, IReadOnlyList<SeedResourceResult> Items)
{
    /// <summary>
    /// Gets the total number of resources the template attempted to create.
    /// </summary>
    public int TotalCount => Items.Count;

    /// <summary>
    /// Gets the number of resources that were created successfully.
    /// </summary>
    public int SucceededCount => Items.Count(item => item.Succeeded);

    /// <summary>
    /// Gets the number of resources that failed to be created.
    /// </summary>
    public int FailedCount => Items.Count(item => !item.Succeeded);

    /// <summary>
    /// Gets the overall terminal state: <see cref="OperationState.Succeeded"/> when every resource was
    /// created, otherwise <see cref="OperationState.Failed"/>.
    /// </summary>
    public OperationState OverallState => FailedCount == 0 ? OperationState.Succeeded : OperationState.Failed;
}

/// <summary>
/// The result of provisioning a single resource from a seed template.
/// </summary>
/// <param name="ServiceKey">The catalogue key of the owning service, for example <c>sqs</c>.</param>
/// <param name="ResourceType">The human-readable resource type, for example <c>Queue</c>.</param>
/// <param name="Name">The name the resource was created with.</param>
/// <param name="Succeeded">Whether the resource was created successfully.</param>
/// <param name="Error">A human-readable failure reason when <paramref name="Succeeded"/> is <see langword="false"/>; otherwise <see langword="null"/>.</param>
public record SeedResourceResult(
    string ServiceKey,
    string ResourceType,
    string Name,
    bool Succeeded,
    string? Error);
