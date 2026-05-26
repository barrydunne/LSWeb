using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteParameter;

/// <summary>
/// Delete an SSM parameter by name. This is a destructive action that cannot be undone.
/// </summary>
/// <param name="Name">The fully-qualified name of the parameter to delete.</param>
public record DeleteParameterCommand(string Name) : ICommand;
