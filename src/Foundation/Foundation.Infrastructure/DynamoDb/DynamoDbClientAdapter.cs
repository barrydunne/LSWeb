using System.Diagnostics.CodeAnalysis;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Foundation.Infrastructure.Aws;
using DynamoDbCondition = Foundation.Domain.DynamoDb.DynamoDbCondition;
using DynamoDbItem = Foundation.Domain.DynamoDb.DynamoDbItem;
using DynamoDbItemPage = Foundation.Domain.DynamoDb.DynamoDbItemPage;
using DynamoDbQueryRequest = Foundation.Domain.DynamoDb.DynamoDbQueryRequest;
using DynamoDbQueryResult = Foundation.Domain.DynamoDb.DynamoDbQueryResult;
using DynamoDbStatementRequest = Foundation.Domain.DynamoDb.DynamoDbStatementRequest;
using DynamoDbStatementResult = Foundation.Domain.DynamoDb.DynamoDbStatementResult;
using DynamoDbTable = Foundation.Domain.DynamoDb.DynamoDbTable;
using DynamoDbTableDetail = Foundation.Domain.DynamoDb.DynamoDbTableDetail;

namespace Foundation.Infrastructure.DynamoDb;

/// <summary>
/// Reads DynamoDB through the resilient AWS gateway so the same code works against LocalStack or real
/// AWS. All access flows through <see cref="IAwsGateway"/>, which records capability and converts
/// failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class DynamoDbClientAdapter : IDynamoDbClient
{
    private const string ServiceKey = "dynamodb";

    private readonly IAwsGateway _gateway;

    public DynamoDbClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<DynamoDbTable>>> ListTablesAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonDynamoDBClient, IReadOnlyList<DynamoDbTable>>(
            ServiceKey,
            async (client, token) =>
            {
                var tables = new List<DynamoDbTable>();
                string? lastTableName = null;

                do
                {
                    var response = await client.ListTablesAsync(
                        new ListTablesRequest { ExclusiveStartTableName = lastTableName, Limit = 100 },
                        token);

                    foreach (var name in response.TableNames ?? [])
                        tables.Add(new DynamoDbTable(name));

                    lastTableName = response.LastEvaluatedTableName;
                }
                while (!string.IsNullOrEmpty(lastTableName));

                return tables;
            },
            cancellationToken);

    public Task<Result<DynamoDbTableDetail>> DescribeTableAsync(
        string tableName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonDynamoDBClient, DynamoDbTableDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.DescribeTableAsync(
                    new DescribeTableRequest { TableName = tableName },
                    token);

                return DynamoDbTableMapper.ToTableDetail(response.Table);
            },
            cancellationToken);

    public async Task<Result> CreateTableAsync(
        Foundation.Domain.DynamoDb.DynamoDbTableSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonDynamoDBClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CreateTableAsync(BuildCreateRequest(specification), token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteTableAsync(string tableName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonDynamoDBClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteTableAsync(new DeleteTableRequest { TableName = tableName }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<DynamoDbItemPage>> ScanItemsAsync(
        string tableName, int limit, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonDynamoDBClient, DynamoDbItemPage>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.ScanAsync(
                    new ScanRequest { TableName = tableName, Limit = limit }, token);

                var items = (response.Items ?? [])
                    .Select(item => new DynamoDbItem(Document.FromAttributeMap(item).ToJsonPretty()))
                    .ToList();

                return new DynamoDbItemPage(items, response.LastEvaluatedKey is { Count: > 0 });
            },
            cancellationToken);

    public Task<Result<DynamoDbQueryResult>> QueryTableAsync(
        DynamoDbQueryRequest request, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonDynamoDBClient, DynamoDbQueryResult>(
            ServiceKey,
            (client, token) => request.Scan
                ? RunScanAsync(client, request, token)
                : RunQueryAsync(client, request, token),
            cancellationToken);

    public async Task<Result<DynamoDbItem>> GetItemAsync(
        string tableName, string keyJson, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonDynamoDBClient, DynamoDbItem?>(
            ServiceKey,
            async (client, token) =>
            {
                var key = Document.FromJson(keyJson).ToAttributeMap();
                var response = await client.GetItemAsync(
                    new GetItemRequest { TableName = tableName, Key = key, ConsistentRead = true }, token);

                return response.IsItemSet
                    ? new DynamoDbItem(Document.FromAttributeMap(response.Item).ToJsonPretty())
                    : null;
            },
            cancellationToken);

        if (!result.IsSuccess)
            return result.Error!.Value;

        if (result.Value is null)
            return new Error($"Item not found in table '{tableName}'.");

        return result.Value;
    }

    public async Task<Result> PutItemAsync(
        string tableName, string itemJson, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonDynamoDBClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var item = Document.FromJson(itemJson).ToAttributeMap();
                await client.PutItemAsync(new PutItemRequest { TableName = tableName, Item = item }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteItemAsync(
        string tableName, string keyJson, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonDynamoDBClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var key = Document.FromJson(keyJson).ToAttributeMap();
                await client.DeleteItemAsync(new DeleteItemRequest { TableName = tableName, Key = key }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<DynamoDbStatementResult>> ExecuteStatementAsync(
        DynamoDbStatementRequest request, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonDynamoDBClient, DynamoDbStatementResult>(
            ServiceKey,
            async (client, token) =>
            {
                var statementRequest = new ExecuteStatementRequest { Statement = request.Statement };

                if (request.Limit > 0)
                    statementRequest.Limit = request.Limit;

                if (!string.IsNullOrEmpty(request.NextToken))
                    statementRequest.NextToken = request.NextToken;

                var response = await client.ExecuteStatementAsync(statementRequest, token);

                var items = (response.Items ?? [])
                    .Select(item => new DynamoDbItem(Document.FromAttributeMap(item).ToJsonPretty()))
                    .ToList();

                return new DynamoDbStatementResult(
                    items, string.IsNullOrEmpty(response.NextToken) ? null : response.NextToken);
            },
            cancellationToken);

    private static CreateTableRequest BuildCreateRequest(
        Foundation.Domain.DynamoDb.DynamoDbTableSpecification specification)
    {
        var attributeType = ScalarAttributeType.FindValue(specification.PartitionKeyType);
        var attributes = new List<AttributeDefinition>
        {
            new() { AttributeName = specification.PartitionKeyName, AttributeType = attributeType },
        };
        var keySchema = new List<KeySchemaElement>
        {
            new() { AttributeName = specification.PartitionKeyName, KeyType = KeyType.HASH },
        };

        if (!string.IsNullOrEmpty(specification.SortKeyName))
        {
            attributes.Add(new AttributeDefinition
            {
                AttributeName = specification.SortKeyName,
                AttributeType = ScalarAttributeType.FindValue(specification.SortKeyType),
            });
            keySchema.Add(new KeySchemaElement
            {
                AttributeName = specification.SortKeyName,
                KeyType = KeyType.RANGE,
            });
        }

        var request = new CreateTableRequest
        {
            TableName = specification.TableName,
            AttributeDefinitions = attributes,
            KeySchema = keySchema,
            BillingMode = BillingMode.FindValue(specification.BillingMode),
        };

        if (request.BillingMode == BillingMode.PROVISIONED)
        {
            request.ProvisionedThroughput = new ProvisionedThroughput
            {
                ReadCapacityUnits = specification.ReadCapacityUnits ?? 1,
                WriteCapacityUnits = specification.WriteCapacityUnits ?? 1,
            };
        }

        return request;
    }

    private static async Task<DynamoDbQueryResult> RunQueryAsync(
        AmazonDynamoDBClient client, DynamoDbQueryRequest request, CancellationToken token)
    {
        var allocator = new PlaceholderAllocator();
        var names = new Dictionary<string, string>();
        var values = new Dictionary<string, AttributeValue>();

        var queryRequest = new QueryRequest
        {
            TableName = request.TableName,
            KeyConditionExpression = BuildKeyConditionExpression(request, names, values, allocator),
            Limit = request.Limit,
        };

        var filterExpression = BuildFilterExpression(request.Filters, names, values, allocator);
        if (filterExpression is not null)
            queryRequest.FilterExpression = filterExpression;

        if (!string.IsNullOrEmpty(request.IndexName))
            queryRequest.IndexName = request.IndexName;

        if (names.Count > 0)
            queryRequest.ExpressionAttributeNames = names;

        if (values.Count > 0)
            queryRequest.ExpressionAttributeValues = values;

        var startKey = DecodeToken(request.StartToken);
        if (startKey is not null)
            queryRequest.ExclusiveStartKey = startKey;

        var response = await client.QueryAsync(queryRequest, token);
        return ToResult(response.Items, response.LastEvaluatedKey);
    }

    private static async Task<DynamoDbQueryResult> RunScanAsync(
        AmazonDynamoDBClient client, DynamoDbQueryRequest request, CancellationToken token)
    {
        var allocator = new PlaceholderAllocator();
        var names = new Dictionary<string, string>();
        var values = new Dictionary<string, AttributeValue>();

        var scanRequest = new ScanRequest
        {
            TableName = request.TableName,
            Limit = request.Limit,
        };

        var filterExpression = BuildFilterExpression(request.Filters, names, values, allocator);
        if (filterExpression is not null)
            scanRequest.FilterExpression = filterExpression;

        if (!string.IsNullOrEmpty(request.IndexName))
            scanRequest.IndexName = request.IndexName;

        if (names.Count > 0)
            scanRequest.ExpressionAttributeNames = names;

        if (values.Count > 0)
            scanRequest.ExpressionAttributeValues = values;

        var startKey = DecodeToken(request.StartToken);
        if (startKey is not null)
            scanRequest.ExclusiveStartKey = startKey;

        var response = await client.ScanAsync(scanRequest, token);
        return ToResult(response.Items, response.LastEvaluatedKey);
    }

    private static string BuildKeyConditionExpression(
        DynamoDbQueryRequest request,
        Dictionary<string, string> names,
        Dictionary<string, AttributeValue> values,
        PlaceholderAllocator allocator)
    {
        var parts = new List<string>();

        if (request.PartitionKey is not null)
            parts.Add(BuildComparison(request.PartitionKey, names, values, allocator));

        if (request.SortKey is not null)
            parts.Add(BuildComparison(request.SortKey, names, values, allocator));

        return string.Join(" AND ", parts);
    }

    private static string? BuildFilterExpression(
        IReadOnlyList<DynamoDbCondition> filters,
        Dictionary<string, string> names,
        Dictionary<string, AttributeValue> values,
        PlaceholderAllocator allocator)
    {
        if (filters.Count == 0)
            return null;

        var parts = filters
            .Select(filter => BuildComparison(filter, names, values, allocator))
            .ToList();

        return string.Join(" AND ", parts);
    }

    private static string BuildComparison(
        DynamoDbCondition condition,
        Dictionary<string, string> names,
        Dictionary<string, AttributeValue> values,
        PlaceholderAllocator allocator)
    {
        var name = allocator.NextName(names, condition.AttributeName);

        return condition.Operator switch
        {
            "begins_with" =>
                $"begins_with({name}, {allocator.NextValue(values, ToAttributeValue(condition.ValueType, condition.Value))})",
            "contains" =>
                $"contains({name}, {allocator.NextValue(values, ToAttributeValue(condition.ValueType, condition.Value))})",
            "between" =>
                $"{name} BETWEEN {allocator.NextValue(values, ToAttributeValue(condition.ValueType, condition.Value))}"
                + $" AND {allocator.NextValue(values, ToAttributeValue(condition.ValueType, condition.SecondValue ?? string.Empty))}",
            _ =>
                $"{name} {condition.Operator} {allocator.NextValue(values, ToAttributeValue(condition.ValueType, condition.Value))}",
        };
    }

    private static AttributeValue ToAttributeValue(string valueType, string value)
        => valueType switch
        {
            "N" => new AttributeValue { N = value },
            "BOOL" => new AttributeValue { BOOL = bool.TryParse(value, out var parsed) && parsed },
            _ => new AttributeValue { S = value },
        };

    private static Dictionary<string, AttributeValue>? DecodeToken(string? token)
        => string.IsNullOrEmpty(token) ? null : Document.FromJson(token).ToAttributeMap();

    private static DynamoDbQueryResult ToResult(
        List<Dictionary<string, AttributeValue>>? items, Dictionary<string, AttributeValue>? lastKey)
    {
        var mapped = (items ?? [])
            .Select(item => new DynamoDbItem(Document.FromAttributeMap(item).ToJsonPretty()))
            .ToList();

        var nextToken = lastKey is { Count: > 0 }
            ? Document.FromAttributeMap(lastKey).ToJson()
            : null;

        return new DynamoDbQueryResult(mapped, nextToken);
    }

    private sealed class PlaceholderAllocator
    {
        private int _index;

        public string NextName(Dictionary<string, string> names, string attributeName)
        {
            var placeholder = $"#n{_index++}";
            names[placeholder] = attributeName;
            return placeholder;
        }

        public string NextValue(Dictionary<string, AttributeValue> values, AttributeValue value)
        {
            var placeholder = $":v{_index++}";
            values[placeholder] = value;
            return placeholder;
        }
    }
}
