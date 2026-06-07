using Foundation.Domain.Seed;

namespace Foundation.Application.Seed;

/// <summary>
/// The executable plan for a seed template: the template metadata for display together with the
/// ordered steps that provision its resources.
/// </summary>
/// <param name="Template">The template metadata, including the resource descriptors.</param>
/// <param name="Steps">The ordered provisioning steps to dispatch.</param>
internal sealed record SeedTemplatePlan(SeedTemplate Template, IReadOnlyList<SeedActionStep> Steps);
