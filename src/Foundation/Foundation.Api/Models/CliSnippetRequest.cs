namespace Foundation.Api.Models;

/// <summary>
/// A request to generate an AWS CLI snippet for an operation.
/// </summary>
/// <param name="Service">The AWS service name as used by the CLI, for example <c>s3api</c>.</param>
/// <param name="Operation">The CLI operation name, for example <c>list-buckets</c>.</param>
/// <param name="Parameters">The ordered operation parameters; omitted when the operation takes none.</param>
public sealed record CliSnippetRequest(
    string Service,
    string Operation,
    IReadOnlyList<CliSnippetParameterRequest>? Parameters);

/// <summary>
/// A single parameter supplied to an AWS CLI operation.
/// </summary>
/// <param name="Name">The CLI option name without the leading dashes, for example <c>bucket</c>.</param>
/// <param name="Value">The parameter value; replaced with a placeholder when sensitive.</param>
/// <param name="IsSensitive">Whether the value is sensitive and must never be embedded in the snippet.</param>
public sealed record CliSnippetParameterRequest(string Name, string Value, bool IsSensitive);
