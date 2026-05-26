namespace Foundation.Api.Models;

/// <summary>
/// The SSM parameters that live under a requested hierarchy path.
/// </summary>
/// <param name="Path">The hierarchy path that was browsed.</param>
/// <param name="Parameters">The parameter summaries, ordered as returned by the backend.</param>
public sealed record ParameterListResponse(
    string Path,
    IReadOnlyList<ParameterSummaryResponse> Parameters);

/// <summary>
/// A concise view of an SSM parameter as it appears in a parameter list.
/// </summary>
/// <param name="Name">The fully-qualified name (path) of the parameter.</param>
/// <param name="Type">The parameter type, such as String, StringList, or SecureString.</param>
/// <param name="Version">The version number of the parameter.</param>
/// <param name="LastModifiedDate">When the parameter was last modified, if known.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the parameter.</param>
public sealed record ParameterSummaryResponse(
    string Name,
    string Type,
    long Version,
    DateTimeOffset? LastModifiedDate,
    string Arn);

/// <summary>
/// The details required to create or overwrite an SSM parameter.
/// </summary>
/// <param name="Name">The fully-qualified name (path) of the parameter to create.</param>
/// <param name="Type">The parameter type, such as String, StringList, or SecureString.</param>
/// <param name="Value">The parameter value to store.</param>
/// <param name="Description">An optional human-readable description of the parameter.</param>
public sealed record ParameterCreateRequest(
    string Name,
    string Type,
    string Value,
    string? Description);

/// <summary>
/// The current value of an SSM parameter along with the metadata of the version that produced it.
/// </summary>
/// <param name="Name">The fully-qualified name (path) of the parameter.</param>
/// <param name="Type">The parameter type, such as String, StringList, or SecureString.</param>
/// <param name="Version">The version number of the parameter.</param>
/// <param name="Value">The value to display; masked when sensitive unless a reveal was both requested and permitted.</param>
/// <param name="IsSensitive">Whether the parameter is a SecureString whose value is masked by default.</param>
/// <param name="RevealAllowed">Whether the host permits the value to be revealed.</param>
public sealed record ParameterValueResponse(
    string Name,
    string Type,
    long Version,
    string Value,
    bool IsSensitive,
    bool RevealAllowed);

/// <summary>
/// A request to store a new value against an existing SSM parameter.
/// </summary>
/// <param name="Value">The parameter value to store.</param>
public sealed record ParameterValueUpdateRequest(string Value);

/// <summary>
/// The change history held for an SSM parameter.
/// </summary>
/// <param name="Name">The fully-qualified name (path) of the parameter.</param>
/// <param name="RevealAllowed">Whether the host permits the values to be revealed.</param>
/// <param name="Entries">The historical versions of the parameter.</param>
public sealed record ParameterHistoryResponse(
    string Name,
    bool RevealAllowed,
    IReadOnlyList<ParameterHistoryEntryResponse> Entries);

/// <summary>
/// A single historical version of an SSM parameter.
/// </summary>
/// <param name="Type">The parameter type, such as String, StringList, or SecureString.</param>
/// <param name="Version">The version number of the parameter.</param>
/// <param name="Value">The value to display; masked when sensitive unless a reveal was both requested and permitted.</param>
/// <param name="LastModifiedDate">When this version was stored, if known.</param>
/// <param name="LastModifiedUser">The identity that stored this version.</param>
/// <param name="IsSensitive">Whether the parameter is a SecureString whose value is masked by default.</param>
public sealed record ParameterHistoryEntryResponse(
    string Type,
    long Version,
    string Value,
    DateTimeOffset? LastModifiedDate,
    string LastModifiedUser,
    bool IsSensitive);
