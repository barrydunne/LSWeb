using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.DynamoDb;

namespace Foundation.Application.Queries.ExecuteDynamoDbStatement;

/// <summary>
/// Run a PartiQL statement against DynamoDB for a bounded page of items.
/// </summary>
/// <param name="Request">The statement specification, including the page limit and pagination token.</param>
public record ExecuteDynamoDbStatementQuery(DynamoDbStatementRequest Request)
    : IQuery<ExecuteDynamoDbStatementQueryResult>;

/// <summary>
/// The page of DynamoDB items returned by a PartiQL statement.
/// </summary>
/// <param name="Result">The items and pagination token returned by the backend.</param>
public record ExecuteDynamoDbStatementQueryResult(DynamoDbStatementResult Result);
