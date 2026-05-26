namespace Foundation.Domain.Ssm;

/// <summary>
/// The change history held for an SSM parameter along with the parameter's identity.
/// </summary>
/// <param name="Name">The fully-qualified name (path) of the parameter.</param>
/// <param name="Entries">The historical versions of the parameter, newest first.</param>
public sealed record ParameterHistoryList(
    string Name,
    IReadOnlyList<ParameterHistoryEntry> Entries);

/// <summary>
/// A single historical version of an SSM parameter.
/// </summary>
/// <param name="Type">The parameter type, such as String, StringList, or SecureString.</param>
/// <param name="Version">The version number of the parameter.</param>
/// <param name="Value">The stored value, decrypted when the parameter is a SecureString.</param>
/// <param name="LastModifiedDate">When this version was stored, if known.</param>
/// <param name="LastModifiedUser">The identity that stored this version.</param>
public sealed record ParameterHistoryEntry(
    string Type,
    long Version,
    string Value,
    DateTimeOffset? LastModifiedDate,
    string LastModifiedUser);
