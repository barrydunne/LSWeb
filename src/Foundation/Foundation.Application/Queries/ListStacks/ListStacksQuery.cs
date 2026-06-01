using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Queries.ListStacks;

/// <summary>
/// List the CloudFormation stacks available on the backend.
/// </summary>
public record ListStacksQuery : IQuery<ListStacksQueryResult>;

/// <summary>
/// The CloudFormation stacks available on the backend.
/// </summary>
/// <param name="Stacks">The stacks, ordered as returned by the backend.</param>
public record ListStacksQueryResult(IReadOnlyList<CloudFormationStackSummary> Stacks);
