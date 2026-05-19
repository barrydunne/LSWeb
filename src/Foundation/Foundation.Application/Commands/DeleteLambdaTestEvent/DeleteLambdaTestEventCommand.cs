using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteLambdaTestEvent;

/// <summary>
/// Command that deletes a named test event from a Lambda function.
/// </summary>
/// <param name="FunctionName">The name of the function the event belongs to.</param>
/// <param name="Name">The name of the test event to delete.</param>
public record DeleteLambdaTestEventCommand(string FunctionName, string Name) : ICommand;
