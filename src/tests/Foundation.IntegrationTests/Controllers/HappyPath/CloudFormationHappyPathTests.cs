using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for CloudFormation: create a stack from a minimal template, confirm it is
/// listed and retrievable, then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class CloudFormationHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public CloudFormationHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateListGetDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var stackName = $"itest-cfn-{Guid.NewGuid():N}";

        var templateBody = """
            {
              "AWSTemplateFormatVersion": "2010-09-09",
              "Resources": {
                "TestTopic": {
                  "Type": "AWS::SNS::Topic",
                  "Properties": {}
                }
              }
            }
            """;

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/cloudformation/stack",
            new CloudFormationStackCreateRequest(
                stackName,
                templateBody,
                null,
                null,
                null),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var list = await client.GetFromJsonAsync<CloudFormationStackListResponse>(
            "/api/services/cloudformation/stacks", cancellationToken);
        list.Should().NotBeNull();
        list!.Stacks.Should().ContainSingle(stack => stack.StackName == stackName);

        var detail = await client.GetFromJsonAsync<CloudFormationStackDetailResponse>(
            $"/api/services/cloudformation/stack?name={Uri.EscapeDataString(stackName)}", cancellationToken);
        detail.Should().NotBeNull();
        detail!.StackName.Should().Be(stackName);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/cloudformation/stack?name={Uri.EscapeDataString(stackName)}", cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
