namespace Foundation.Domain.Snippets;

/// <summary>
/// Describes an AWS operation for which an equivalent CLI snippet can be generated.
/// </summary>
/// <param name="Service">The AWS service name as used by the CLI, for example <c>s3api</c>.</param>
/// <param name="Operation">The CLI operation name, for example <c>list-buckets</c>.</param>
/// <param name="Parameters">The ordered operation parameters.</param>
public sealed record CliOperation(string Service, string Operation, IReadOnlyList<CliParameter> Parameters);
