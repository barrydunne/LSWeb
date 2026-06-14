using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateLambdaFunctionUrl;

/// <summary>
/// Create an HTTP function URL for a Lambda function.
/// </summary>
/// <param name="FunctionName">The name of the function.</param>
/// <param name="AuthType">The authentication mode, either <c>NONE</c> or <c>AWS_IAM</c>.</param>
public record CreateLambdaFunctionUrlCommand(string FunctionName, string AuthType) : ICommand;
