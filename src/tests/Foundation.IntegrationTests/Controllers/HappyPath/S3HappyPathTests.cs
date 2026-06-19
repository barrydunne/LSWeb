using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for S3: create a bucket, confirm it appears in the listing, then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class S3HappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public S3HappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateListDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var bucketName = $"itest-s3-{Guid.NewGuid():N}".ToLowerInvariant();

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/s3/buckets",
            new S3BucketCreateRequest(bucketName),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var list = await client.GetFromJsonAsync<S3BucketListResponse>(
            "/api/services/s3/buckets",
            cancellationToken);
        list.Should().NotBeNull();
        list!.Buckets.Should().ContainSingle(bucket => bucket.Name == bucketName);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/s3/buckets/{Uri.EscapeDataString(bucketName)}",
            cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
