using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Queries.DescribeChangeSet;

/// <summary>
/// Describe a single CloudFormation change set, including the resource changes it would apply.
/// </summary>
/// <param name="StackName">The name or Amazon Resource Name of the stack the change set targets.</param>
/// <param name="ChangeSetName">The name or Amazon Resource Name of the change set to describe.</param>
public record DescribeChangeSetQuery(string StackName, string ChangeSetName)
    : IQuery<DescribeChangeSetQueryResult>;

/// <summary>
/// The detail of a single CloudFormation change set.
/// </summary>
/// <param name="ChangeSet">The change set detail.</param>
public record DescribeChangeSetQueryResult(ChangeSetDetail ChangeSet);
