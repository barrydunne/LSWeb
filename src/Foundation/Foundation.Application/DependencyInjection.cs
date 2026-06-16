using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AspNet.KickStarter.CQRS;
using FluentValidation;
using Foundation.Application.Preferences;
using Foundation.Application.Seed;
using Foundation.Application.Snapshot;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Application;

/// <summary>
/// Registration of the application layer services with the dependency injection container.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
public static class DependencyInjection
{
    /// <summary>
    /// Register the application layer services, including the MediatR request pipeline and validators.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddFoundationApplication(this IServiceCollection services)
        => services
            .AddMediatR(_ => _.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
            .AddValidationPipelineBehavior()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true)
            .AddSingleton<ISeedTemplateCatalogue, SeedTemplateCatalogue>()
            .AddSingleton<IWorkspaceSnapshotExporter, WorkspaceSnapshotExporter>()
            .AddSingleton<IWorkspaceSnapshotImporter, WorkspaceSnapshotImporter>()
            .AddSingleton<IRecentlyViewedPruner, RecentlyViewedPruner>();
}
