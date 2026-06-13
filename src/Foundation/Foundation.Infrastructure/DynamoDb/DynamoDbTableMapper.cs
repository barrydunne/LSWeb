using Amazon.DynamoDBv2.Model;
using Foundation.Domain.DynamoDb;

namespace Foundation.Infrastructure.DynamoDb;

/// <summary>
/// Translates AWS DynamoDB table description shapes into the domain records the application works
/// with, applying safe defaults for missing values.
/// </summary>
internal static class DynamoDbTableMapper
{
    /// <summary>
    /// Map an SDK table description to the domain table detail.
    /// </summary>
    /// <param name="table">The SDK table description returned by a describe call.</param>
    /// <param name="ttl">The SDK time-to-live description returned by a describe-time-to-live call, if available.</param>
    /// <returns>The domain table detail.</returns>
    public static DynamoDbTableDetail ToTableDetail(TableDescription table, TimeToLiveDescription? ttl = null)
        => new(
            table.TableName ?? string.Empty,
            table.TableArn ?? string.Empty,
            table.TableStatus?.Value ?? string.Empty,
            table.ItemCount ?? 0,
            table.TableSizeBytes ?? 0,
            table.BillingModeSummary?.BillingMode?.Value,
            table.ProvisionedThroughput?.ReadCapacityUnits,
            table.ProvisionedThroughput?.WriteCapacityUnits,
            table.CreationDateTime is null
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(table.CreationDateTime.Value, DateTimeKind.Utc)),
            ToKeySchema(table.KeySchema),
            ToAttributeDefinitions(table.AttributeDefinitions),
            ToGlobalSecondaryIndexes(table.GlobalSecondaryIndexes),
            ToLocalSecondaryIndexes(table.LocalSecondaryIndexes),
            table.StreamSpecification?.StreamEnabled ?? false,
            table.StreamSpecification?.StreamViewType?.Value,
            string.IsNullOrEmpty(table.LatestStreamArn) ? null : table.LatestStreamArn,
            ttl?.TimeToLiveStatus?.Value,
            string.IsNullOrEmpty(ttl?.AttributeName) ? null : ttl.AttributeName);

    private static List<DynamoDbKeyElement> ToKeySchema(List<KeySchemaElement>? keySchema)
        => (keySchema ?? [])
            .Select(element => new DynamoDbKeyElement(
                element.AttributeName ?? string.Empty,
                element.KeyType?.Value ?? string.Empty))
            .ToList();

    private static List<DynamoDbAttributeDefinition> ToAttributeDefinitions(
        List<AttributeDefinition>? attributes)
        => (attributes ?? [])
            .Select(attribute => new DynamoDbAttributeDefinition(
                attribute.AttributeName ?? string.Empty,
                attribute.AttributeType?.Value ?? string.Empty))
            .ToList();

    private static List<DynamoDbSecondaryIndex> ToGlobalSecondaryIndexes(
        List<GlobalSecondaryIndexDescription>? indexes)
        => (indexes ?? [])
            .Select(index => new DynamoDbSecondaryIndex(
                index.IndexName ?? string.Empty,
                index.IndexStatus?.Value,
                ToKeySchema(index.KeySchema)))
            .ToList();

    private static List<DynamoDbSecondaryIndex> ToLocalSecondaryIndexes(
        List<LocalSecondaryIndexDescription>? indexes)
        => (indexes ?? [])
            .Select(index => new DynamoDbSecondaryIndex(
                index.IndexName ?? string.Empty,
                null,
                ToKeySchema(index.KeySchema)))
            .ToList();
}
