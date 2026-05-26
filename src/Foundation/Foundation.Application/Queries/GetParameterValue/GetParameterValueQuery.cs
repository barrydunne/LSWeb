using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.GetParameterValue;

/// <summary>
/// Get the current value of an SSM parameter, masking the value when the parameter is a SecureString
/// unless a guarded reveal is requested and permitted by the host.
/// </summary>
/// <param name="Name">The fully-qualified name of the parameter to read.</param>
/// <param name="Reveal">Whether the caller has explicitly requested the unmasked value.</param>
public record GetParameterValueQuery(string Name, bool Reveal) : IQuery<GetParameterValueQueryResult>;

/// <summary>
/// The current value of an SSM parameter, with the value masked as required.
/// </summary>
/// <param name="Name">The fully-qualified name of the parameter.</param>
/// <param name="Type">The parameter type, such as String, StringList, or SecureString.</param>
/// <param name="Version">The version number of the parameter.</param>
/// <param name="Value">The value to display; masked when sensitive unless a reveal was both requested and permitted.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the parameter.</param>
/// <param name="IsSensitive">Whether the parameter is a SecureString whose value is masked by default.</param>
/// <param name="RevealAllowed">Whether the host permits the value to be revealed.</param>
public record GetParameterValueQueryResult(
    string Name,
    string Type,
    long Version,
    string Value,
    string Arn,
    bool IsSensitive,
    bool RevealAllowed);
