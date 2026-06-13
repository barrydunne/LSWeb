using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;
namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class DynamoDbControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public DynamoDbControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task ListTables_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/dynamodb/tables", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<DynamoDbTableListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Tables.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetTable_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/dynamodb/tables/missing-table", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<DynamoDbTableDetailResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateTable_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DynamoDbTableCreateRequest(
            "integration-create-table",
            "pk",
            "S",
            null,
            null,
            "PAY_PER_REQUEST",
            null,
            null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/dynamodb/tables", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task DeleteTable_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            "/api/services/dynamodb/tables/missing-table", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ScanItems_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/dynamodb/tables/missing-table/items?limit=10",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<DynamoDbItemListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Items.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetItem_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var key = Uri.EscapeDataString("{\"pk\":{\"S\":\"missing\"}}");

        // Act
        var response = await client.GetAsync(
            $"/api/services/dynamodb/tables/missing-table/item?key={key}",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<DynamoDbItemResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task PutItem_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DynamoDbItemPutRequest("{\"pk\":{\"S\":\"integration-item\"}}", null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/dynamodb/tables/missing-table/items",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task DeleteItem_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var key = Uri.EscapeDataString("{\"pk\":{\"S\":\"missing\"}}");

        // Act
        var response = await client.DeleteAsync(
            $"/api/services/dynamodb/tables/missing-table/item?key={key}",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task QueryTable_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DynamoDbQueryRequestBody(
            null,
            false,
            new DynamoDbQueryConditionRequest("pk", "=", "S", "missing", null),
            null,
            null,
            10,
            null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/dynamodb/tables/missing-table/query",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<DynamoDbQueryResultResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Items.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ScanTable_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DynamoDbQueryRequestBody(
            null,
            true,
            null,
            null,
            [new DynamoDbQueryConditionRequest("status", "contains", "S", "OPEN", null)],
            10,
            null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/dynamodb/tables/missing-table/query",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ExecuteStatement_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DynamoDbStatementRequestBody(
            "SELECT * FROM \"missing-table\"",
            10,
            null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/dynamodb/statement",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<DynamoDbStatementResultResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Items.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task PutTtl_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DynamoDbTtlUpdateRequest(true, "expiresAt");

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/services/dynamodb/tables/missing-table/ttl",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task CreateIndex_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DynamoDbIndexCreateRequest("gsi-1", "gpk", "S", null, null, "ALL");

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/dynamodb/tables/missing-table/indexes",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task DeleteIndex_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            "/api/services/dynamodb/tables/missing-table/indexes/gsi-1",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ExecuteTransaction_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DynamoDbTransactionRequestBody(
        [
            new DynamoDbTransactionActionRequest("Put", "missing-table", "{\"pk\":{\"S\":\"a\"}}"),
        ]);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/dynamodb/transaction",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task BatchWrite_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DynamoDbBatchWriteRequestBody(
        [
            new DynamoDbBatchWriteItemRequest("Put", "missing-table", "{\"pk\":{\"S\":\"a\"}}"),
        ]);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/dynamodb/batch/write",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task BatchGet_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new DynamoDbBatchGetRequestBody(
        [
            new DynamoDbBatchGetKeyRequest("missing-table", "{\"pk\":{\"S\":\"a\"}}"),
        ]);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/dynamodb/batch/get",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }
}

