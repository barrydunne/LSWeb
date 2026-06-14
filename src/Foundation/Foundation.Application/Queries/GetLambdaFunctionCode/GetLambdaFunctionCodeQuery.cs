using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Queries.GetLambdaFunctionCode;

/// <summary>
/// Get the deployed package and entry-point details of a single Lambda function.
/// </summary>
/// <param name="FunctionName">The name of the function to retrieve.</param>
public record GetLambdaFunctionCodeQuery(string FunctionName) : IQuery<GetLambdaFunctionCodeQueryResult>;

/// <summary>
/// The deployed code details of the requested Lambda function.
/// </summary>
/// <param name="Code">The function code details.</param>
public record GetLambdaFunctionCodeQueryResult(LambdaFunctionCode Code);
