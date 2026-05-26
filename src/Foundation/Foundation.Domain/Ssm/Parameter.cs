namespace Foundation.Domain.Ssm;

/// <summary>
/// A concise view of an SSM parameter as it appears in a parameter list.
/// </summary>
/// <param name="Name">The fully-qualified name (path) of the parameter.</param>
/// <param name="Type">The parameter type, such as String, StringList, or SecureString.</param>
/// <param name="Version">The version number of the parameter.</param>
/// <param name="LastModifiedDate">When the parameter was last modified, if known.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the parameter.</param>
public sealed record Parameter(
    string Name,
    string Type,
    long Version,
    DateTimeOffset? LastModifiedDate,
    string Arn);
