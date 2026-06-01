using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Queries.ListStackResources;

/// <summary>
/// List the resources a single CloudFormation stack manages.
/// </summary>
/// <param name="StackName">The name or Amazon Resource Name of the stack whose resources to list.</param>
public record ListStackResourcesQuery(string StackName) : IQuery<ListStackResourcesQueryResult>;

/// <summary>
/// The resources a single CloudFormation stack manages.
/// </summary>
/// <param name="Resources">The stack resources.</param>
public record ListStackResourcesQueryResult(IReadOnlyList<StackResource> Resources);
