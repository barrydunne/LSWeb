using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Queries.GetLambdaFunction;

/// <summary>
/// Get the full configuration of a single Lambda function.
/// </summary>
/// <param name="FunctionName">The name of the function to retrieve.</param>
public record GetLambdaFunctionQuery(string FunctionName) : IQuery<GetLambdaFunctionQueryResult>;

/// <summary>
/// The configuration of the requested Lambda function.
/// </summary>
/// <param name="Function">The function detail.</param>
public record GetLambdaFunctionQueryResult(LambdaFunctionDetail Function);
