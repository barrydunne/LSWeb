using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Queries.ListChangeSets;

/// <summary>
/// List the change sets pending against a single CloudFormation stack.
/// </summary>
/// <param name="StackName">The name or Amazon Resource Name of the stack whose change sets to list.</param>
public record ListChangeSetsQuery(string StackName) : IQuery<ListChangeSetsQueryResult>;

/// <summary>
/// The change sets pending against a single CloudFormation stack.
/// </summary>
/// <param name="ChangeSets">The change sets.</param>
public record ListChangeSetsQueryResult(IReadOnlyList<ChangeSetSummary> ChangeSets);
