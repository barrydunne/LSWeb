using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class S3ControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public S3ControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task ListBuckets_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/services/s3/buckets", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<S3BucketListResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Buckets.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateBucket_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/s3/buckets",
            new S3BucketCreateRequest("integration-test-bucket"),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task DeleteBucket_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            "/api/services/s3/buckets/integration-test-bucket", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }
}
