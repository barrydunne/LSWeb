using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Queries.GetStack;

/// <summary>
/// Get the details of a single CloudFormation stack.
/// </summary>
/// <param name="StackName">The name or Amazon Resource Name of the stack to describe.</param>
public record GetStackQuery(string StackName) : IQuery<GetStackQueryResult>;

/// <summary>
/// The details of a single CloudFormation stack.
/// </summary>
/// <param name="Stack">The stack details.</param>
public record GetStackQueryResult(CloudFormationStackDetail Stack);
