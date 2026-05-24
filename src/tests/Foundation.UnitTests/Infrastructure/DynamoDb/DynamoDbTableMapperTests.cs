using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Foundation.Infrastructure.DynamoDb;

namespace Foundation.UnitTests.Infrastructure.DynamoDb;

public class DynamoDbTableMapperTests
{
    [Fact]
    public void ToTableDetail_MapsAllProperties()
    {
        // Arrange
        var creation = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Unspecified);
        var table = new TableDescription
        {
            TableName = "orders",
            TableArn = "arn:orders",
            TableStatus = TableStatus.ACTIVE,
            ItemCount = 5,
            TableSizeBytes = 1024,
            BillingModeSummary = new BillingModeSummary { BillingMode = BillingMode.PAY_PER_REQUEST },
            ProvisionedThroughput = new ProvisionedThroughputDescription
            {
                ReadCapacityUnits = 10,
                WriteCapacityUnits = 20,
            },
            CreationDateTime = creation,
            KeySchema =
            [
                new KeySchemaElement { AttributeName = "id", KeyType = KeyType.HASH },
            ],
            AttributeDefinitions =
            [
                new AttributeDefinition { AttributeName = "id", AttributeType = ScalarAttributeType.S },
            ],
            GlobalSecondaryIndexes =
            [
                new GlobalSecondaryIndexDescription
                {
                    IndexName = "gsi-1",
                    IndexStatus = IndexStatus.ACTIVE,
                    KeySchema = [new KeySchemaElement { AttributeName = "gid", KeyType = KeyType.HASH }],
                },
            ],
            LocalSecondaryIndexes =
            [
                new LocalSecondaryIndexDescription
                {
                    IndexName = "lsi-1",
                    KeySchema = [new KeySchemaElement { AttributeName = "lid", KeyType = KeyType.RANGE }],
                },
            ],
            StreamSpecification = new StreamSpecification
            {
                StreamEnabled = true,
                StreamViewType = StreamViewType.NEW_AND_OLD_IMAGES,
            },
            LatestStreamArn = "arn:aws:dynamodb:eu-west-1:000000000000:table/orders/stream/2024-01-02T03:04:05.000",
        };

        // Act
        var detail = DynamoDbTableMapper.ToTableDetail(table);

        // Assert
        detail.Name.Should().Be("orders");
        detail.Arn.Should().Be("arn:orders");
        detail.Status.Should().Be("ACTIVE");
        detail.ItemCount.Should().Be(5);
        detail.TableSizeBytes.Should().Be(1024);
        detail.BillingMode.Should().Be("PAY_PER_REQUEST");
        detail.ReadCapacityUnits.Should().Be(10);
        detail.WriteCapacityUnits.Should().Be(20);
        detail.CreatedAt.Should().Be(new DateTimeOffset(creation, TimeSpan.Zero));
        detail.KeySchema.Should().ContainSingle(_ => _.AttributeName == "id" && _.KeyType == "HASH");
        detail.Attributes.Should().ContainSingle(_ => _.AttributeName == "id" && _.AttributeType == "S");
        detail.StreamEnabled.Should().BeTrue();
        detail.StreamViewType.Should().Be("NEW_AND_OLD_IMAGES");
        detail.LatestStreamArn.Should().Be("arn:aws:dynamodb:eu-west-1:000000000000:table/orders/stream/2024-01-02T03:04:05.000");
        var gsi = detail.GlobalSecondaryIndexes.Should().ContainSingle().Subject;
        gsi.Name.Should().Be("gsi-1");
        gsi.Status.Should().Be("ACTIVE");
        gsi.KeySchema.Should().ContainSingle(_ => _.AttributeName == "gid");
        var lsi = detail.LocalSecondaryIndexes.Should().ContainSingle().Subject;
        lsi.Name.Should().Be("lsi-1");
        lsi.Status.Should().BeNull();
        lsi.KeySchema.Should().ContainSingle(_ => _.AttributeName == "lid");
    }

    [Fact]
    public void ToTableDetail_WhenValuesNull_AppliesDefaults()
    {
        // Arrange
        var table = new TableDescription();

        // Act
        var detail = DynamoDbTableMapper.ToTableDetail(table);

        // Assert
        detail.Name.Should().BeEmpty();
        detail.Arn.Should().BeEmpty();
        detail.Status.Should().BeEmpty();
        detail.ItemCount.Should().Be(0);
        detail.TableSizeBytes.Should().Be(0);
        detail.BillingMode.Should().BeNull();
        detail.ReadCapacityUnits.Should().BeNull();
        detail.WriteCapacityUnits.Should().BeNull();
        detail.CreatedAt.Should().BeNull();
        detail.KeySchema.Should().BeEmpty();
        detail.Attributes.Should().BeEmpty();
        detail.GlobalSecondaryIndexes.Should().BeEmpty();
        detail.LocalSecondaryIndexes.Should().BeEmpty();
        detail.StreamEnabled.Should().BeFalse();
        detail.StreamViewType.Should().BeNull();
        detail.LatestStreamArn.Should().BeNull();
    }

    [Fact]
    public void ToTableDetail_WhenKeyElementValuesNull_AppliesDefaults()
    {
        // Arrange
        var table = new TableDescription
        {
            BillingModeSummary = new BillingModeSummary(),
            KeySchema = [new KeySchemaElement()],
            AttributeDefinitions = [new AttributeDefinition()],
            GlobalSecondaryIndexes = [new GlobalSecondaryIndexDescription()],
            LocalSecondaryIndexes = [new LocalSecondaryIndexDescription()],
        };

        // Act
        var detail = DynamoDbTableMapper.ToTableDetail(table);

        // Assert
        detail.BillingMode.Should().BeNull();
        detail.KeySchema.Should().ContainSingle(_ => _.AttributeName == string.Empty && _.KeyType == string.Empty);
        detail.Attributes.Should().ContainSingle(_ => _.AttributeName == string.Empty && _.AttributeType == string.Empty);
        var gsi = detail.GlobalSecondaryIndexes.Should().ContainSingle().Subject;
        gsi.Name.Should().BeEmpty();
        gsi.Status.Should().BeNull();
        var lsi = detail.LocalSecondaryIndexes.Should().ContainSingle().Subject;
        lsi.Name.Should().BeEmpty();
    }

    [Fact]
    public void ToTableDetail_WhenStreamEnabledWithoutArn_MapsEnabledStateAndNullArn()
    {
        // Arrange
        var table = new TableDescription
        {
            StreamSpecification = new StreamSpecification { StreamEnabled = true },
            LatestStreamArn = string.Empty,
        };

        // Act
        var detail = DynamoDbTableMapper.ToTableDetail(table);

        // Assert
        detail.StreamEnabled.Should().BeTrue();
        detail.StreamViewType.Should().BeNull();
        detail.LatestStreamArn.Should().BeNull();
    }
}
