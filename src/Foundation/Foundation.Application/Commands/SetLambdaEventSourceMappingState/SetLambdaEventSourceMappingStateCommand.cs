using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.SetLambdaEventSourceMappingState;

/// <summary>
/// Enable or disable an event source mapping that triggers a Lambda function.
/// </summary>
/// <param name="FunctionName">The name of the function the mapping triggers, used for messaging.</param>
/// <param name="Uuid">The unique identifier of the mapping to update.</param>
/// <param name="Enabled">Whether the mapping should be enabled.</param>
public record SetLambdaEventSourceMappingStateCommand(string FunctionName, string Uuid, bool Enabled) : ICommand;
