namespace Foundation.Domain.Ssm;

/// <summary>
/// The resolved value of an SSM parameter along with the metadata of the version that produced it.
/// </summary>
/// <param name="Name">The fully-qualified name (path) of the parameter.</param>
/// <param name="Type">The parameter type, such as String, StringList, or SecureString.</param>
/// <param name="Version">The version number of the parameter.</param>
/// <param name="Value">The stored parameter value, decrypted when the parameter is a SecureString.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the parameter.</param>
public sealed record ParameterValue(
    string Name,
    string Type,
    long Version,
    string Value,
    string Arn);

/// <summary>
/// The details required to store a new value against an existing SSM parameter.
/// </summary>
/// <param name="Name">The fully-qualified name (path) of the parameter to update.</param>
/// <param name="Value">The parameter value to store.</param>
public sealed record ParameterValueSpecification(string Name, string Value);
