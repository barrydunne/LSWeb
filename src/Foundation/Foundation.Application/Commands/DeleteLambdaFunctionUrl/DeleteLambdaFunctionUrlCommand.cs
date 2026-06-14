using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteLambdaFunctionUrl;

/// <summary>
/// Delete the HTTP function URL configuration of a Lambda function.
/// </summary>
/// <param name="FunctionName">The name of the function.</param>
public record DeleteLambdaFunctionUrlCommand(string FunctionName) : ICommand;
