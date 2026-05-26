using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateParameterValue;

/// <summary>
/// Store a new value against an existing SSM parameter, creating a new version while preserving the
/// parameter's existing type.
/// </summary>
/// <param name="Name">The fully-qualified name of the parameter to update.</param>
/// <param name="Value">The parameter value to store.</param>
public record UpdateParameterValueCommand(string Name, string Value) : ICommand;
