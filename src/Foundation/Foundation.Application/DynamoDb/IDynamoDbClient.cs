using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.DynamoDb;

namespace Foundation.Application.DynamoDb;

/// <summary>
/// Provides access to AWS DynamoDB: listing the tables on the configured backend and describing an
/// individual table's key schema, throughput, status, and secondary indexes.
/// </summary>
public interface IDynamoDbClient
{
    /// <summary>
    /// Lists the tables available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The tables, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<DynamoDbTable>>> ListTablesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Describes a single table, including its key schema, throughput, status, and secondary indexes.
    /// </summary>
    /// <param name="tableName">The name of the table to describe.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The table description, or an error if the table is missing or the backend could not be reached.</returns>
    Task<Result<DynamoDbTableDetail>> DescribeTableAsync(string tableName, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new table with the supplied key schema and billing configuration.
    /// </summary>
    /// <param name="specification">The table to create, including its key schema and billing mode.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the table could not be created.</returns>
    Task<Result> CreateTableAsync(DynamoDbTableSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a table and all of the items it contains.
    /// </summary>
    /// <param name="tableName">The name of the table to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the table is missing or the backend could not be reached.</returns>
    Task<Result> DeleteTableAsync(string tableName, CancellationToken cancellationToken);

    /// <summary>
    /// Scans a table for a bounded page of items, each rendered as a JSON document.
    /// </summary>
    /// <param name="tableName">The name of the table to scan.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A page of items, or an error if the table is missing or the backend could not be reached.</returns>
    Task<Result<DynamoDbItemPage>> ScanItemsAsync(string tableName, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Queries or scans a table or secondary index for a bounded page of items, each rendered as a JSON document.
    /// </summary>
    /// <param name="request">The query specification, including key conditions, filters, index, limit, and pagination token.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A page of items with a pagination token, or an error if the table is missing or the backend could not be reached.</returns>
    Task<Result<DynamoDbQueryResult>> QueryTableAsync(DynamoDbQueryRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Reads a single item by its primary key.
    /// </summary>
    /// <param name="tableName">The name of the table to read from.</param>
    /// <param name="keyJson">The primary key as a JSON document containing the key attributes.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The item as a JSON document, or an error if the item is missing or the backend could not be reached.</returns>
    Task<Result<DynamoDbItem>> GetItemAsync(string tableName, string keyJson, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or replaces an item from its full JSON representation.
    /// </summary>
    /// <param name="tableName">The name of the table to write to.</param>
    /// <param name="itemJson">The full item as a JSON document.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the item could not be written.</returns>
    Task<Result> PutItemAsync(string tableName, string itemJson, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a single item by its primary key.
    /// </summary>
    /// <param name="tableName">The name of the table to delete from.</param>
    /// <param name="keyJson">The primary key as a JSON document containing the key attributes.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the item could not be deleted.</returns>
    Task<Result> DeleteItemAsync(string tableName, string keyJson, CancellationToken cancellationToken);

    /// <summary>
    /// Runs a PartiQL statement against the configured backend, returning a page of items for a read
    /// statement, or an empty page for a write statement.
    /// </summary>
    /// <param name="request">The statement to execute, including the page limit and pagination token.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A page of items with a pagination token, or an error if the statement failed or the backend could not be reached.</returns>
    Task<Result<DynamoDbStatementResult>> ExecuteStatementAsync(
        DynamoDbStatementRequest request, CancellationToken cancellationToken);
}
