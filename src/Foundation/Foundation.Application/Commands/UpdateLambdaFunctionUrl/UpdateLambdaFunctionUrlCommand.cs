using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateLambdaFunctionUrl;

/// <summary>
/// Update the authentication mode of a Lambda function's HTTP function URL.
/// </summary>
/// <param name="FunctionName">The name of the function.</param>
/// <param name="AuthType">The new authentication mode, either <c>NONE</c> or <c>AWS_IAM</c>.</param>
public record UpdateLambdaFunctionUrlCommand(string FunctionName, string AuthType) : ICommand;
