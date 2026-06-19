using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for IAM: create a role with a trust policy, confirm it is listed and
/// retrievable, then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class IamHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public IamHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateListGetDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var roleName = $"itest-iam-{Guid.NewGuid():N}";

        var trustPolicyJson = JsonSerializer.Serialize(new
        {
            Version = "2012-10-17",
            Statement = new[]
            {
                new
                {
                    Effect = "Allow",
                    Principal = new { Service = "lambda.amazonaws.com" },
                    Action = "sts:AssumeRole",
                },
            },
        });

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/iam/roles",
            new IamRoleCreateRequest(roleName, trustPolicyJson, null, null, null),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var list = await client.GetFromJsonAsync<IamRoleListResponse>(
            "/api/services/iam/roles", cancellationToken);
        list.Should().NotBeNull();
        list!.Roles.Should().ContainSingle(role => role.RoleName == roleName);

        var detail = await client.GetFromJsonAsync<IamRoleDetailResponse>(
            $"/api/services/iam/roles/{Uri.EscapeDataString(roleName)}", cancellationToken);
        detail.Should().NotBeNull();
        detail!.RoleName.Should().Be(roleName);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/iam/roles/{Uri.EscapeDataString(roleName)}", cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
