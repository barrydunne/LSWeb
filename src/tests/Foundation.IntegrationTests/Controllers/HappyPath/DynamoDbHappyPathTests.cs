using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for DynamoDB: create a single-key table, confirm it is listed and exposes
/// its key schema, then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class DynamoDbHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public DynamoDbHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateListGetDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var tableName = $"itest-dynamodb-{Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/dynamodb/tables",
            new DynamoDbTableCreateRequest(
                tableName,
                "id",
                "S",
                null,
                null,
                "PAY_PER_REQUEST",
                null,
                null),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var list = await client.GetFromJsonAsync<DynamoDbTableListResponse>(
            "/api/services/dynamodb/tables", cancellationToken);
        list.Should().NotBeNull();
        list!.Tables.Should().ContainSingle(table => table.Name == tableName);

        var detailResponse = await client.GetAsync(
            $"/api/services/dynamodb/tables/{tableName}", cancellationToken);
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await detailResponse.Content.ReadFromJsonAsync<DynamoDbTableDetailResponse>(
            cancellationToken);
        detail.Should().NotBeNull();
        detail!.Name.Should().Be(tableName);
        detail.KeySchema.Should().ContainSingle(key => key.AttributeName == "id" && key.KeyType == "HASH");

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/dynamodb/tables/{tableName}", cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
