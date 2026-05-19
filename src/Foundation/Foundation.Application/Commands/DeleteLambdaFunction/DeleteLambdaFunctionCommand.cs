using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteLambdaFunction;

/// <summary>
/// Delete a Lambda function.
/// </summary>
/// <param name="FunctionName">The name of the function to delete.</param>
public record DeleteLambdaFunctionCommand(string FunctionName) : ICommand;
