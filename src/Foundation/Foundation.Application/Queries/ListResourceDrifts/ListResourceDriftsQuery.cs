using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Queries.ListResourceDrifts;

/// <summary>
/// List the per-resource drift results recorded against a single CloudFormation stack.
/// </summary>
/// <param name="StackName">The name or Amazon Resource Name of the stack whose resource drifts to list.</param>
public record ListResourceDriftsQuery(string StackName) : IQuery<ListResourceDriftsQueryResult>;

/// <summary>
/// The per-resource drift results recorded against a single CloudFormation stack.
/// </summary>
/// <param name="Drifts">The per-resource drifts.</param>
public record ListResourceDriftsQueryResult(IReadOnlyList<StackResourceDrift> Drifts);
