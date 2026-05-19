using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Queries.ListLambdaFunctions;

/// <summary>
/// List the Lambda functions available on the configured backend.
/// </summary>
public record ListLambdaFunctionsQuery : IQuery<ListLambdaFunctionsQueryResult>;

/// <summary>
/// The Lambda functions returned by the backend.
/// </summary>
/// <param name="Functions">The function summaries, ordered as returned by the backend.</param>
public record ListLambdaFunctionsQueryResult(IReadOnlyList<LambdaFunctionSummary> Functions);
