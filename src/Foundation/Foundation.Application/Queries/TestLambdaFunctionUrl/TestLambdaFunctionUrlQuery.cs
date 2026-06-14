using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Queries.TestLambdaFunctionUrl;

/// <summary>
/// Issue a test HTTP request against a Lambda function's configured URL.
/// </summary>
/// <param name="FunctionName">The name of the function whose URL should be tested.</param>
public record TestLambdaFunctionUrlQuery(string FunctionName) : IQuery<TestLambdaFunctionUrlQueryResult>;

/// <summary>
/// The outcome of the function URL test request.
/// </summary>
/// <param name="Test">The status and body returned by the function URL.</param>
public record TestLambdaFunctionUrlQueryResult(LambdaFunctionUrlTest Test);
