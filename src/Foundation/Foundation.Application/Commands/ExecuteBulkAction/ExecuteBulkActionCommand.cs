using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.Bulk;

namespace Foundation.Application.Commands.ExecuteBulkAction;

/// <summary>
/// Command that applies a single action to a set of resources, isolating each resource so that a
/// failure for one does not prevent the others from being processed, and reporting per-item results.
/// </summary>
/// <param name="Action">The action to apply, for example <c>delete</c>.</param>
/// <param name="ResourceIds">The identifiers of the resources to apply the action to.</param>
public record ExecuteBulkActionCommand(string Action, IReadOnlyList<string> ResourceIds)
    : ICommand<BulkActionOutcome>;
