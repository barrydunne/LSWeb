namespace Foundation.Domain.Bulk;

/// <summary>
/// The result of applying a bulk action to a single resource.
/// </summary>
/// <param name="ResourceId">The identifier of the resource the action targeted.</param>
/// <param name="Succeeded">Whether the action completed successfully for this resource.</param>
/// <param name="Error">A human-readable failure reason when <paramref name="Succeeded"/> is <see langword="false"/>; otherwise <see langword="null"/>.</param>
public record BulkActionItemResult(string ResourceId, bool Succeeded, string? Error);
