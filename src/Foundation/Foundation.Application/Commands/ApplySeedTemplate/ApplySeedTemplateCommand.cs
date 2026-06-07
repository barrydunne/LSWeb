using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.Seed;

namespace Foundation.Application.Commands.ApplySeedTemplate;

/// <summary>
/// Command that applies a seed template by provisioning each of its resources, isolating each step so
/// that a failure for one resource does not prevent the others from being created, and reporting
/// per-resource results.
/// </summary>
/// <param name="TemplateId">The identifier of the seed template to apply.</param>
public record ApplySeedTemplateCommand(string TemplateId) : ICommand<SeedOutcome>;
