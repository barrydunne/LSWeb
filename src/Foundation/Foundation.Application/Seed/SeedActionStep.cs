using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.Seed;

namespace Foundation.Application.Seed;

/// <summary>
/// A single provisioning step within a seed template: a resource descriptor for display paired with
/// the command that creates the resource.
/// </summary>
/// <param name="Descriptor">The descriptor shown to the user for this resource.</param>
/// <param name="Command">The command dispatched to provision the resource.</param>
internal sealed record SeedActionStep(SeedResourceDescriptor Descriptor, ICommand Command);
