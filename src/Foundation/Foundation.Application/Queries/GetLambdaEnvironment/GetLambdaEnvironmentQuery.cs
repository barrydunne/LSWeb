using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Queries.GetLambdaEnvironment;

/// <summary>
/// Get the environment variables of a Lambda function, masking sensitive values unless a guarded
/// reveal is requested.
/// </summary>
/// <param name="FunctionName">The name of the function to read.</param>
/// <param name="Reveal">Whether the caller has explicitly requested unmasked sensitive values.</param>
public record GetLambdaEnvironmentQuery(string FunctionName, bool Reveal) : IQuery<GetLambdaEnvironmentQueryResult>;

/// <summary>
/// The environment variables of the requested Lambda function.
/// </summary>
/// <param name="Variables">The environment variables ordered by name, with sensitive values masked as required.</param>
/// <param name="RevealAllowed">Whether the host permits sensitive values to be revealed.</param>
public record GetLambdaEnvironmentQueryResult(IReadOnlyList<LambdaEnvironmentVariable> Variables, bool RevealAllowed);
