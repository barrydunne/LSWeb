using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Queries.GetLambdaFunctionUrl;

/// <summary>
/// Get the HTTP function URL configuration of a Lambda function.
/// </summary>
/// <param name="FunctionName">The name of the function to read.</param>
public record GetLambdaFunctionUrlQuery(string FunctionName) : IQuery<GetLambdaFunctionUrlQueryResult>;

/// <summary>
/// The function URL configuration of the requested Lambda function.
/// </summary>
/// <param name="Url">The URL configuration, or <see langword="null"/> when no URL is configured.</param>
public record GetLambdaFunctionUrlQueryResult(LambdaFunctionUrl? Url);
