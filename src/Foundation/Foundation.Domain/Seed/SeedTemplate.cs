namespace Foundation.Domain.Seed;

/// <summary>
/// A named, one-click template that provisions a small, coherent set of sample resources so that a
/// developer can populate an empty environment without crafting each resource by hand.
/// </summary>
/// <param name="Id">The stable identifier used to request that this template be applied.</param>
/// <param name="Name">The human-readable name shown on the seed button.</param>
/// <param name="Description">A short summary of what the template provisions.</param>
/// <param name="Resources">The resources the template creates, in the order they are provisioned.</param>
public record SeedTemplate(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<SeedResourceDescriptor> Resources);

/// <summary>
/// Describes a single resource that a <see cref="SeedTemplate"/> provisions, for display before the
/// template is applied.
/// </summary>
/// <param name="ServiceKey">The catalogue key of the owning service, for example <c>sqs</c>.</param>
/// <param name="ResourceType">The human-readable resource type, for example <c>Queue</c>.</param>
/// <param name="Name">The name the resource will be created with.</param>
public record SeedResourceDescriptor(string ServiceKey, string ResourceType, string Name);
