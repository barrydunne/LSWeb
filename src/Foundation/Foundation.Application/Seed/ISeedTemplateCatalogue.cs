using Foundation.Domain.Seed;

namespace Foundation.Application.Seed;

/// <summary>
/// Supplies the catalogue of available seed templates and resolves a template to its executable plan.
/// </summary>
internal interface ISeedTemplateCatalogue
{
    /// <summary>
    /// Gets the available seed templates in display order.
    /// </summary>
    /// <returns>The seed templates.</returns>
    IReadOnlyList<SeedTemplate> GetTemplates();

    /// <summary>
    /// Resolves a template identifier to its executable plan.
    /// </summary>
    /// <param name="templateId">The identifier of the template to resolve.</param>
    /// <returns>The plan when the template exists; otherwise <see langword="null"/>.</returns>
    SeedTemplatePlan? GetPlan(string templateId);
}
