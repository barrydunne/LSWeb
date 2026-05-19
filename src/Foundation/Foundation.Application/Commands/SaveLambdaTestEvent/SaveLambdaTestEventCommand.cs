using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.SaveLambdaTestEvent;

/// <summary>
/// Command that saves a named test event for a Lambda function, replacing any existing event with
/// the same name.
/// </summary>
/// <param name="FunctionName">The name of the function the event belongs to.</param>
/// <param name="Name">The name that identifies the test event within the function.</param>
/// <param name="Payload">The JSON payload to store for the event.</param>
public record SaveLambdaTestEventCommand(string FunctionName, string Name, string Payload) : ICommand;
