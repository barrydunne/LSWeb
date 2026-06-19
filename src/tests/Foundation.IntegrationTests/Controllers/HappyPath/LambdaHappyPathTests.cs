using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for Lambda: create a function from a minimal in-memory zip package,
/// confirm it is listed and retrievable, then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class LambdaHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public LambdaHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateListGetDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var functionName = $"itest-lambda-{Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/lambda/functions",
            new LambdaFunctionCreateRequest(
                functionName,
                "python3.12",
                "index.handler",
                "arn:aws:iam::000000000000:role/itest-lambda-role",
                "Created by integration test",
                128,
                30,
                CreateMinimalZipArchive()),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var detailResponse = await client.GetAsync(
            $"/api/services/lambda/functions/{functionName}", cancellationToken);
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await detailResponse.Content.ReadFromJsonAsync<LambdaFunctionResponse>(cancellationToken);
        detail.Should().NotBeNull();
        detail!.FunctionName.Should().Be(functionName);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/lambda/functions/{functionName}", cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static string CreateMinimalZipArchive()
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("index.py");
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream);
            writer.Write("def handler(event, context):\n    return {'statusCode': 200}\n");
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }
}
