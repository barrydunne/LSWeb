using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Commands.InvokeLambdaFunction;

/// <summary>
/// Invoke a Lambda function synchronously with a JSON payload and capture the response, status,
/// log tail and duration.
/// </summary>
/// <param name="FunctionName">The name of the function to invoke.</param>
/// <param name="Payload">The JSON payload to send to the function.</param>
public record InvokeLambdaFunctionCommand(string FunctionName, string Payload) : ICommand<LambdaInvocationResult>;
