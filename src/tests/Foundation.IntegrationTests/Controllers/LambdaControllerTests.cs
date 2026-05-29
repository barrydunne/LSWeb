using System.Net;
using System.Net.Http.Json;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class LambdaControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public LambdaControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task ListFunctions_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/services/lambda/functions", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<LambdaFunctionListResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Functions.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetFunction_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/services/lambda/functions/process-orders", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<LambdaFunctionResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.FunctionName.Should().NotBeNull();
        }
        else
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task GetEnvironment_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/services/lambda/functions/process-orders/environment?reveal=false", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<LambdaEnvironmentResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Variables.Should().NotBeNull();
        }
        else
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task UpdateEnvironment_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new LambdaEnvironmentUpdateRequest([new LambdaEnvironmentVariableRequest("STAGE", "test")]);

        // Act
        var response = await client.PutAsJsonAsync("/api/services/lambda/functions/process-orders/environment", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task Invoke_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new LambdaInvokeRequest("{}");

        // Act
        var response = await client.PostAsJsonAsync("/api/services/lambda/functions/process-orders/invocations", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<LambdaInvocationResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
        }
        else
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task CreateFunction_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new LambdaFunctionCreateRequest(
            "new-function",
            "dotnet8",
            "index.handler",
            "arn:aws:iam::000000000000:role/lambda",
            "A new function",
            128,
            3,
            "QkFTRTY0");

        // Act
        var response = await client.PostAsJsonAsync("/api/services/lambda/functions", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task UpdateFunction_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new LambdaFunctionUpdateRequest(
            "dotnet8",
            "index.handler",
            "arn:aws:iam::000000000000:role/lambda",
            "An updated function",
            256,
            15,
            null);

        // Act
        var response = await client.PutAsJsonAsync("/api/services/lambda/functions/process-orders", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task DeleteFunction_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/services/lambda/functions/process-orders", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task ListTestEvents_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/services/lambda/functions/process-orders/test-events", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<LambdaTestEventListResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Templates.Should().NotBeEmpty();
        }
        else
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task SaveTestEvent_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new LambdaTestEventSaveRequest("integration-event", "{\"hello\":\"world\"}");

        // Act
        var response = await client.PutAsJsonAsync("/api/services/lambda/functions/process-orders/test-events", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task DeleteTestEvent_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/services/lambda/functions/process-orders/test-events/integration-event", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task ListEventSourceMappings_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/services/lambda/functions/process-orders/event-source-mappings", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<LambdaEventSourceMappingListResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Mappings.Should().NotBeNull();
            payload.S3Triggers.Should().NotBeNull();
        }
        else
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task SetEventSourceMappingState_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new LambdaEventSourceMappingStateRequest(false);

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/services/lambda/functions/process-orders/event-source-mappings/00000000-0000-0000-0000-000000000000/state",
            request,
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
    public async Task ListLogEvents_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/services/lambda/functions/process-orders/logs?limit=50", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<LambdaLogEventListResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
        }
        else
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task GetInvocationInsights_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/services/lambda/functions/process-orders/invocation-insights?limit=50", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<LambdaInvocationInsightsResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
        }
        else
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    [Fact]
    public async Task ListLayers_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/services/lambda/functions/process-orders/layers", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<LambdaLayerListResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
        }
        else
        {
            ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
        }
    }

    private sealed record LambdaTestEventListResponse(
        IReadOnlyList<LambdaTestEventResponse> Events,
        IReadOnlyList<LambdaTestEventResponse> Templates);

    private sealed record LambdaTestEventResponse(string Name, string Payload);

    private sealed record LambdaTestEventSaveRequest(string Name, string? Payload);

    private sealed record LambdaEventSourceMappingListResponse(
        IReadOnlyList<LambdaEventSourceMappingResponse> Mappings,
        IReadOnlyList<LambdaS3TriggerResponse> S3Triggers);

    private sealed record LambdaEventSourceMappingResponse(
        string Uuid,
        string EventSourceArn,
        string FunctionArn,
        string State,
        int BatchSize,
        string LastModified);

    private sealed record LambdaS3TriggerResponse(string BucketArn);

    private sealed record LambdaEventSourceMappingStateRequest(bool Enabled);

    private sealed record LambdaLogEventListResponse(
        string LogGroupName,
        IReadOnlyList<LambdaLogEventResponse> Events);

    private sealed record LambdaLogEventResponse(
        string Timestamp,
        string Message,
        string LogStreamName);

    private sealed record LambdaInvocationInsightsResponse(
        string LogGroupName,
        LambdaInvocationMetricsResponse Metrics,
        IReadOnlyList<LambdaRecentInvocationResponse> RecentInvocations);

    private sealed record LambdaInvocationMetricsResponse(
        int InvocationCount,
        int ErrorCount,
        double AverageDurationMs,
        double MaxDurationMs);

    private sealed record LambdaRecentInvocationResponse(
        string RequestId,
        string Timestamp,
        double DurationMs,
        bool HasError);

    private sealed record LambdaLayerListResponse(
        IReadOnlyList<LambdaLayerResponse> Layers);

    private sealed record LambdaLayerResponse(
        string Arn,
        string Name,
        string Version);

    private sealed record LambdaEnvironmentResponse(IReadOnlyList<LambdaEnvironmentVariableResponse> Variables, bool RevealAllowed);

    private sealed record LambdaEnvironmentVariableResponse(string Name, string Value, bool IsSensitive);

    private sealed record LambdaEnvironmentUpdateRequest(IReadOnlyList<LambdaEnvironmentVariableRequest> Variables);

    private sealed record LambdaEnvironmentVariableRequest(string Name, string Value);

    private sealed record LambdaInvokeRequest(string? Payload);

    private sealed record LambdaInvocationResponse(int StatusCode, string Payload, string LogTail, string FunctionError, long DurationMs);

    private sealed record LambdaFunctionCreateRequest(
        string FunctionName,
        string Runtime,
        string Handler,
        string Role,
        string Description,
        int MemorySize,
        int Timeout,
        string ZipFileBase64);

    private sealed record LambdaFunctionUpdateRequest(
        string Runtime,
        string Handler,
        string Role,
        string Description,
        int MemorySize,
        int Timeout,
        string? ZipFileBase64);

    private sealed record LambdaFunctionListResponse(IReadOnlyList<LambdaFunctionSummaryResponse> Functions);

    private sealed record LambdaFunctionSummaryResponse(
        string FunctionName,
        string Runtime,
        string Description,
        string LastModified,
        int MemorySize,
        int Timeout);

    private sealed record LambdaFunctionResponse(
        string FunctionName,
        string FunctionArn,
        string Runtime,
        string Handler,
        string Description,
        string LastModified,
        int MemorySize,
        int Timeout,
        string Role);
}
