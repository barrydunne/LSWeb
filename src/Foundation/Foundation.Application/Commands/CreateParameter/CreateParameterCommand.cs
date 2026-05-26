using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateParameter;

/// <summary>
/// Create or overwrite an SSM parameter with the supplied name, type, value, and description.
/// </summary>
/// <param name="Name">The fully-qualified name (path) of the parameter to create.</param>
/// <param name="Type">The parameter type, such as String, StringList, or SecureString.</param>
/// <param name="Value">The parameter value to store.</param>
/// <param name="Description">An optional human-readable description of the parameter.</param>
public record CreateParameterCommand(
    string Name,
    string Type,
    string Value,
    string? Description) : ICommand;
