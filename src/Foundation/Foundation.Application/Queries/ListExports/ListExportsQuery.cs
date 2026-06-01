using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Queries.ListExports;

/// <summary>
/// List the exported output values published across all CloudFormation stacks.
/// </summary>
public record ListExportsQuery() : IQuery<ListExportsQueryResult>;

/// <summary>
/// The exported output values published across all CloudFormation stacks.
/// </summary>
/// <param name="Exports">The exports.</param>
public record ListExportsQueryResult(IReadOnlyList<StackExport> Exports);
