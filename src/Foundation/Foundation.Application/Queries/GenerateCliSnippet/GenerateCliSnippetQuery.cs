using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Snippets;

namespace Foundation.Application.Queries.GenerateCliSnippet;

/// <summary>
/// Generate an AWS CLI snippet that reproduces an operation against the configured endpoint.
/// </summary>
/// <param name="Service">The AWS service name as used by the CLI, for example <c>s3api</c>.</param>
/// <param name="Operation">The CLI operation name, for example <c>list-buckets</c>.</param>
/// <param name="Parameters">The ordered operation parameters; sensitive values are emitted as placeholders.</param>
public record GenerateCliSnippetQuery(string Service, string Operation, IReadOnlyList<CliParameter> Parameters)
    : IQuery<GenerateCliSnippetQueryResult>;

/// <summary>
/// The result of generating an AWS CLI snippet.
/// </summary>
/// <param name="Command">The generated <c>aws</c> CLI command.</param>
public record GenerateCliSnippetQueryResult(string Command);
