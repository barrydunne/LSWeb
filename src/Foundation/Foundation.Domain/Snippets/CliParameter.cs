namespace Foundation.Domain.Snippets;

/// <summary>
/// A single named parameter supplied to an AWS CLI operation.
/// </summary>
/// <param name="Name">The CLI option name without the leading dashes, for example <c>bucket</c>.</param>
/// <param name="Value">The parameter value; substituted with a placeholder when sensitive.</param>
/// <param name="IsSensitive">Whether the value is sensitive and must never be embedded in the snippet.</param>
public sealed record CliParameter(string Name, string Value, bool IsSensitive = false);
