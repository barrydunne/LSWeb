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

    [Fact]
    public async Task ListObjects_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/s3/buckets/integration-test-bucket/objects?prefix=", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<S3ObjectListingResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Prefixes.Should().NotBeNull();
            payload.Objects.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateFolder_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/s3/buckets/integration-test-bucket/folders",
            new S3FolderCreateRequest("integration-test-folder/"),
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
    public async Task UploadObject_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([1, 2, 3]);
        content.Add(fileContent, "file", "integration-test.txt");
        content.Add(new StringContent(string.Empty), "prefix");

        // Act
        var response = await client.PostAsync(
            "/api/services/s3/buckets/integration-test-bucket/objects", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task DownloadObject_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/s3/buckets/integration-test-bucket/objects/content?key=integration-test.txt",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task PreviewObject_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/s3/buckets/integration-test-bucket/objects/preview?key=integration-test.txt",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task PresignObject_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/s3/buckets/integration-test-bucket/objects/presign?key=integration-test.txt&expirySeconds=900",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task DeleteObject_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            "/api/services/s3/buckets/integration-test-bucket/objects?key=integration-test.txt",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task GetObjectMetadata_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/s3/buckets/integration-test-bucket/objects/metadata?key=integration-test.txt",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task UpdateObjectTags_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/services/s3/buckets/integration-test-bucket/objects/tags?key=integration-test.txt",
            new S3ObjectTagsUpdateRequest(new Dictionary<string, string> { ["stage"] = "prod" }),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task CopyObject_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/s3/buckets/integration-test-bucket/objects/copy?key=integration-test.txt",
            new S3ObjectCopyRequest("integration-test-bucket", "copies/integration-test.txt"),
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
    public async Task MoveObject_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/s3/buckets/integration-test-bucket/objects/move?key=integration-test.txt",
            new S3ObjectCopyRequest("integration-test-bucket", "moved/integration-test.txt"),
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
    public async Task GetBucketConfiguration_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/s3/buckets/integration-test-bucket/configuration",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<S3BucketConfigurationResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.LifecycleRules.Should().NotBeNull();
            payload.Notifications.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetBucketStorageSummary_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/s3/buckets/integration-test-bucket/storage-summary",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<S3BucketStorageSummaryResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.ObjectCount.Should().BeGreaterThanOrEqualTo(0);
            payload.TotalSizeBytes.Should().BeGreaterThanOrEqualTo(0);
        }
    }
}
