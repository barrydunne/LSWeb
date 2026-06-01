using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Queries.ListStackEvents;

/// <summary>
/// List the chronological events recorded against a single CloudFormation stack.
/// </summary>
/// <param name="StackName">The name or Amazon Resource Name of the stack whose events to list.</param>
public record ListStackEventsQuery(string StackName) : IQuery<ListStackEventsQueryResult>;

/// <summary>
/// The events recorded against a single CloudFormation stack, newest first.
/// </summary>
/// <param name="Events">The stack events.</param>
public record ListStackEventsQueryResult(IReadOnlyList<StackEvent> Events);
