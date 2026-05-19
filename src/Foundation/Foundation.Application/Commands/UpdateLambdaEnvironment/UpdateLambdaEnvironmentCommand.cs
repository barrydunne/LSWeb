using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateLambdaEnvironment;

/// <summary>
/// Replace the environment variables of a Lambda function. Sensitive values left masked are
/// preserved so existing secrets are never overwritten with the mask sentinel.
/// </summary>
/// <param name="FunctionName">The name of the function to update.</param>
/// <param name="Variables">The full set of environment variables to apply, keyed by name.</param>
public record UpdateLambdaEnvironmentCommand(string FunctionName, IReadOnlyDictionary<string, string> Variables) : ICommand;
