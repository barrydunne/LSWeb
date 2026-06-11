using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class CloudFormationControllerTests
{
    private const string MinimalTemplate =
        "{\"AWSTemplateFormatVersion\":\"2010-09-09\",\"Resources\":{\"Topic\":{\"Type\":\"AWS::SNS::Topic\"}}}";

    private readonly IntegrationTestsFixture _fixture;

    public CloudFormationControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task ValidateTemplate_WhenSuppliedInlineBody_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new CloudFormationTemplateValidationRequest(MinimalTemplate, null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/cloudformation/template/validate", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<CloudFormationTemplateValidationResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Capabilities.Should().NotBeNull();
            payload.Parameters.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateStack_WhenSuppliedInlineTemplate_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var stackName = $"itest-stack-{Guid.NewGuid():N}";
        var request = new CloudFormationStackCreateRequest(stackName, MinimalTemplate, null, null, null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/cloudformation/stack", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            var payload = await response.Content.ReadFromJsonAsync<CloudFormationStackOperationResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.StackId.Should().NotBeNull();
        }
    }
}
