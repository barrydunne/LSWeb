namespace Foundation.Domain.Ssm;

/// <summary>
/// The details required to create or overwrite an SSM parameter.
/// </summary>
/// <param name="Name">The fully-qualified name (path) of the parameter to create.</param>
/// <param name="Type">The parameter type, such as String, StringList, or SecureString.</param>
/// <param name="Value">The parameter value to store.</param>
/// <param name="Description">An optional human-readable description of the parameter.</param>
public sealed record ParameterSpecification(
    string Name,
    string Type,
    string Value,
    string? Description);
