using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.ListImports;

/// <summary>
/// List the names of the CloudFormation stacks that import a single exported value.
/// </summary>
/// <param name="ExportName">The export name whose importing stacks to list.</param>
public record ListImportsQuery(string ExportName) : IQuery<ListImportsQueryResult>;

/// <summary>
/// The names of the CloudFormation stacks that import a single exported value.
/// </summary>
/// <param name="ImportingStackNames">The names of the importing stacks.</param>
public record ListImportsQueryResult(IReadOnlyList<string> ImportingStackNames);
