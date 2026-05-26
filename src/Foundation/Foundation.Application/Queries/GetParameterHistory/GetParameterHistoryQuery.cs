using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.GetParameterHistory;

/// <summary>
/// Get the change history of an SSM parameter, masking each value when the parameter is a
/// SecureString unless a guarded reveal is requested and permitted by the host.
/// </summary>
/// <param name="Name">The fully-qualified name of the parameter to read.</param>
/// <param name="Reveal">Whether the caller has explicitly requested the unmasked values.</param>
public record GetParameterHistoryQuery(string Name, bool Reveal) : IQuery<GetParameterHistoryQueryResult>;

/// <summary>
/// The change history of an SSM parameter, with each value masked as required.
/// </summary>
/// <param name="Name">The fully-qualified name of the parameter.</param>
/// <param name="RevealAllowed">Whether the host permits the values to be revealed.</param>
/// <param name="Entries">The historical versions of the parameter.</param>
public record GetParameterHistoryQueryResult(
    string Name,
    bool RevealAllowed,
    IReadOnlyList<GetParameterHistoryEntryResult> Entries);

/// <summary>
/// A single historical version of an SSM parameter, with the value masked as required.
/// </summary>
/// <param name="Type">The parameter type, such as String, StringList, or SecureString.</param>
/// <param name="Version">The version number of the parameter.</param>
/// <param name="Value">The value to display; masked when sensitive unless a reveal was both requested and permitted.</param>
/// <param name="LastModifiedDate">When this version was stored, if known.</param>
/// <param name="LastModifiedUser">The identity that stored this version.</param>
/// <param name="IsSensitive">Whether the parameter is a SecureString whose value is masked by default.</param>
public record GetParameterHistoryEntryResult(
    string Type,
    long Version,
    string Value,
    DateTimeOffset? LastModifiedDate,
    string LastModifiedUser,
    bool IsSensitive);
