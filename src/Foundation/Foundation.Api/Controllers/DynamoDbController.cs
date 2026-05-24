using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateDynamoDbTable;
using Foundation.Application.Commands.DeleteDynamoDbItem;
using Foundation.Application.Commands.DeleteDynamoDbTable;
using Foundation.Application.Commands.PutDynamoDbItem;
using Foundation.Application.Queries.ExecuteDynamoDbStatement;
using Foundation.Application.Queries.GetDynamoDbItem;
using Foundation.Application.Queries.GetDynamoDbTable;
using Foundation.Application.Queries.ListDynamoDbTables;
using Foundation.Application.Queries.QueryDynamoDbTable;
using Foundation.Application.Queries.ScanDynamoDbItems;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to AWS DynamoDB: listing the tables on the configured backend and viewing the
/// details of an individual table.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/dynamodb")]
public partial class DynamoDbController : ControllerBase
{
    private const int DefaultScanLimit = 25;
    private const int MaxScanLimit = 100;

    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamoDbController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public DynamoDbController(ISender sender, ILogger<DynamoDbController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the DynamoDB tables available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the table summaries.</returns>
    [HttpGet("tables")]
    [ProducesResponseType(typeof(DynamoDbTableListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListTables(CancellationToken cancellationToken)
    {
        LogHandlingListTables();
        var result = await _sender.Send(new ListDynamoDbTablesQuery(), cancellationToken);
        LogListTablesHandled(result.IsSuccess);
        return result.Match(
            tables => Results.Ok(new DynamoDbTableListResponse(
                tables.Tables
                    .Select(table => new DynamoDbTableResponse(table.Name))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Describes a single DynamoDB table, including its key schema, throughput, status, and secondary
    /// indexes.
    /// </summary>
    /// <param name="tableName">The name of the table to describe.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the table description.</returns>
    [HttpGet("tables/{tableName}")]
    [ProducesResponseType(typeof(DynamoDbTableDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetTable(string tableName, CancellationToken cancellationToken)
    {
        LogHandlingGetTable(tableName);
        var result = await _sender.Send(new GetDynamoDbTableQuery(tableName), cancellationToken);
        LogGetTableHandled(result.IsSuccess);
        return result.Match(
            table => Results.Ok(ToResponse(table.Table)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new DynamoDB table with the supplied key schema and billing configuration.
    /// </summary>
    /// <param name="request">The table to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created table.</returns>
    [HttpPost("tables")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateTable(
        [FromBody] DynamoDbTableCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateTable(request.TableName);
        var result = await _sender.Send(
            new CreateDynamoDbTableCommand(
                request.TableName,
                request.PartitionKeyName,
                request.PartitionKeyType,
                request.SortKeyName,
                request.SortKeyType,
                request.BillingMode,
                request.ReadCapacityUnits,
                request.WriteCapacityUnits),
            cancellationToken);
        LogCreateTableHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/dynamodb/tables/{Uri.EscapeDataString(request.TableName)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a DynamoDB table and all of the items it contains. This is a destructive action that
    /// cannot be undone.
    /// </summary>
    /// <param name="tableName">The name of the table to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("tables/{tableName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteTable(string tableName, CancellationToken cancellationToken)
    {
        LogHandlingDeleteTable(tableName);
        var result = await _sender.Send(new DeleteDynamoDbTableCommand(tableName), cancellationToken);
        LogDeleteTableHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Scans a DynamoDB table for a bounded page of items, each rendered as a JSON document.
    /// </summary>
    /// <param name="tableName">The name of the table to scan.</param>
    /// <param name="limit">The maximum number of items to return; clamped to the range 1 to 100.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the page of items.</returns>
    [HttpGet("tables/{tableName}/items")]
    [ProducesResponseType(typeof(DynamoDbItemListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ScanItems(
        string tableName, [FromQuery] int limit, CancellationToken cancellationToken)
    {
        var effectiveLimit = limit <= 0 ? DefaultScanLimit : Math.Min(limit, MaxScanLimit);
        LogHandlingScanItems(tableName, effectiveLimit);
        var result = await _sender.Send(
            new ScanDynamoDbItemsQuery(tableName, effectiveLimit), cancellationToken);
        LogScanItemsHandled(result.IsSuccess);
        return result.Match(
            scan => Results.Ok(new DynamoDbItemListResponse(
                scan.Page.Items
                    .Select(item => new DynamoDbItemResponse(item.Json))
                    .ToList(),
                scan.Page.Truncated)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Queries or scans a DynamoDB table or secondary index for a bounded page of items, applying key
    /// conditions and filter expressions, and returning a pagination token for retrieving further pages.
    /// </summary>
    /// <param name="tableName">The name of the table to read from.</param>
    /// <param name="request">The query specification, including key conditions, filters, index, and pagination token.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the page of items and the pagination token.</returns>
    [HttpPost("tables/{tableName}/query")]
    [ProducesResponseType(typeof(DynamoDbQueryResultResponse), StatusCodes.Status200OK)]
    public async Task<IResult> QueryTable(
        string tableName, [FromBody] DynamoDbQueryRequestBody request, CancellationToken cancellationToken)
    {
        var effectiveLimit = request.Limit <= 0 ? DefaultScanLimit : Math.Min(request.Limit, MaxScanLimit);
        LogHandlingQueryTable(tableName, request.Scan, effectiveLimit);
        var query = new QueryDynamoDbTableQuery(
            new Domain.DynamoDb.DynamoDbQueryRequest(
                tableName,
                request.IndexName,
                request.Scan,
                request.PartitionKey is null ? null : ToCondition(request.PartitionKey),
                request.SortKey is null ? null : ToCondition(request.SortKey),
                (request.Filters ?? []).Select(ToCondition).ToList(),
                effectiveLimit,
                request.StartToken));
        var result = await _sender.Send(query, cancellationToken);
        LogQueryTableHandled(result.IsSuccess);
        return result.Match(
            page => Results.Ok(new DynamoDbQueryResultResponse(
                page.Page.Items
                    .Select(item => new DynamoDbItemResponse(item.Json))
                    .ToList(),
                page.Page.NextToken)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Runs a PartiQL statement against DynamoDB, returning a bounded page of items for a read statement
    /// and a pagination token for retrieving further pages.
    /// </summary>
    /// <param name="request">The statement to execute, including the page limit and pagination token.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the page of items and the pagination token.</returns>
    [HttpPost("statement")]
    [ProducesResponseType(typeof(DynamoDbStatementResultResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ExecuteStatement(
        [FromBody] DynamoDbStatementRequestBody request, CancellationToken cancellationToken)
    {
        var effectiveLimit = request.Limit <= 0 ? DefaultScanLimit : Math.Min(request.Limit, MaxScanLimit);
        LogHandlingExecuteStatement(effectiveLimit);
        var query = new ExecuteDynamoDbStatementQuery(
            new Domain.DynamoDb.DynamoDbStatementRequest(request.Statement, effectiveLimit, request.NextToken));
        var result = await _sender.Send(query, cancellationToken);
        LogExecuteStatementHandled(result.IsSuccess);
        return result.Match(
            page => Results.Ok(new DynamoDbStatementResultResponse(
                page.Result.Items
                    .Select(item => new DynamoDbItemResponse(item.Json))
                    .ToList(),
                page.Result.NextToken)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Reads a single DynamoDB item by its primary key.
    /// </summary>
    /// <param name="tableName">The name of the table to read from.</param>
    /// <param name="key">The primary key as a JSON document containing the key attributes.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the item.</returns>
    [HttpGet("tables/{tableName}/item")]
    [ProducesResponseType(typeof(DynamoDbItemResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetItem(
        string tableName, [FromQuery] string key, CancellationToken cancellationToken)
    {
        LogHandlingGetItem(tableName);
        var result = await _sender.Send(new GetDynamoDbItemQuery(tableName, key), cancellationToken);
        LogGetItemHandled(result.IsSuccess);
        return result.Match(
            item => Results.Ok(new DynamoDbItemResponse(item.Item.Json)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates or replaces a DynamoDB item from its full JSON representation.
    /// </summary>
    /// <param name="tableName">The name of the table to write to.</param>
    /// <param name="request">The item to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result on success.</returns>
    [HttpPost("tables/{tableName}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IResult> PutItem(
        string tableName, [FromBody] DynamoDbItemPutRequest request, CancellationToken cancellationToken)
    {
        LogHandlingPutItem(tableName);
        var result = await _sender.Send(
            new PutDynamoDbItemCommand(tableName, request.Item), cancellationToken);
        LogPutItemHandled(result.IsSuccess);
        return result.Match(
            () => Results.Ok(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a single DynamoDB item by its primary key. This is a destructive action that cannot be
    /// undone.
    /// </summary>
    /// <param name="tableName">The name of the table to delete from.</param>
    /// <param name="key">The primary key as a JSON document containing the key attributes.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("tables/{tableName}/item")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteItem(
        string tableName, [FromQuery] string key, CancellationToken cancellationToken)
    {
        LogHandlingDeleteItem(tableName);
        var result = await _sender.Send(
            new DeleteDynamoDbItemCommand(tableName, key), cancellationToken);
        LogDeleteItemHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    private static DynamoDbTableDetailResponse ToResponse(Domain.DynamoDb.DynamoDbTableDetail table)
        => new(
            table.Name,
            table.Arn,
            table.Status,
            table.ItemCount,
            table.TableSizeBytes,
            table.BillingMode,
            table.ReadCapacityUnits,
            table.WriteCapacityUnits,
            table.CreatedAt,
            table.KeySchema
                .Select(key => new DynamoDbKeyAttributeResponse(key.AttributeName, key.KeyType))
                .ToList(),
            table.Attributes
                .Select(attribute => new DynamoDbAttributeResponse(attribute.AttributeName, attribute.AttributeType))
                .ToList(),
            table.GlobalSecondaryIndexes
                .Select(ToIndexResponse)
                .ToList(),
            table.LocalSecondaryIndexes
                .Select(ToIndexResponse)
                .ToList(),
            table.StreamEnabled,
            table.StreamViewType,
            table.LatestStreamArn);

    private static DynamoDbSecondaryIndexResponse ToIndexResponse(Domain.DynamoDb.DynamoDbSecondaryIndex index)
        => new(
            index.Name,
            index.Status,
            index.KeySchema
                .Select(key => new DynamoDbKeyAttributeResponse(key.AttributeName, key.KeyType))
                .ToList());

    private static Domain.DynamoDb.DynamoDbCondition ToCondition(DynamoDbQueryConditionRequest request)
        => new(
            request.AttributeName,
            request.Operator,
            request.ValueType,
            request.Value,
            request.SecondValue);

    [LoggerMessage(LogLevel.Trace, "Handling DynamoDB table list request.")]
    private partial void LogHandlingListTables();

    [LoggerMessage(LogLevel.Trace, "DynamoDB table list request handled. Success: {Success}")]
    private partial void LogListTablesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling DynamoDB table describe request for {TableName}.")]
    private partial void LogHandlingGetTable(string tableName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB table describe request handled. Success: {Success}")]
    private partial void LogGetTableHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling DynamoDB table create request for {TableName}.")]
    private partial void LogHandlingCreateTable(string tableName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB table create request handled. Success: {Success}")]
    private partial void LogCreateTableHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling DynamoDB table delete request for {TableName}.")]
    private partial void LogHandlingDeleteTable(string tableName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB table delete request handled. Success: {Success}")]
    private partial void LogDeleteTableHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling DynamoDB item scan request for {TableName} (limit {Limit}).")]
    private partial void LogHandlingScanItems(string tableName, int limit);

    [LoggerMessage(LogLevel.Trace, "DynamoDB item scan request handled. Success: {Success}")]
    private partial void LogScanItemsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling DynamoDB query request for {TableName} (scan {Scan}, limit {Limit}).")]
    private partial void LogHandlingQueryTable(string tableName, bool scan, int limit);

    [LoggerMessage(LogLevel.Trace, "DynamoDB query request handled. Success: {Success}")]
    private partial void LogQueryTableHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling DynamoDB PartiQL statement request (limit {Limit}).")]
    private partial void LogHandlingExecuteStatement(int limit);

    [LoggerMessage(LogLevel.Trace, "DynamoDB PartiQL statement request handled. Success: {Success}")]
    private partial void LogExecuteStatementHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling DynamoDB get item request for {TableName}.")]
    private partial void LogHandlingGetItem(string tableName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB get item request handled. Success: {Success}")]
    private partial void LogGetItemHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling DynamoDB put item request for {TableName}.")]
    private partial void LogHandlingPutItem(string tableName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB put item request handled. Success: {Success}")]
    private partial void LogPutItemHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling DynamoDB delete item request for {TableName}.")]
    private partial void LogHandlingDeleteItem(string tableName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB delete item request handled. Success: {Success}")]
    private partial void LogDeleteItemHandled(bool success);
}
