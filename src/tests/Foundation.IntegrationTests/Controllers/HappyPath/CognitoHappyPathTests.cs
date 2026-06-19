using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for Cognito (Pro feature, skipped on community): create a user pool,
/// confirm it is listed and retrievable, then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class CognitoHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public CognitoHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateListGetDelete_RoundTripsSuccessfully()
    {
        _fixture.SkipIfLocalStackNotPro();

        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var poolName = $"itest-cognito-{Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/cognito/user-pools",
            new UserPoolCreateRequest(
                poolName,
                "OFF",
                ["email"],
                ["email"],
                new PasswordPolicyModel(8, true, true, true, false)),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<UserPoolCreatedResponse>(cancellationToken);
        created.Should().NotBeNull();
        var poolId = created!.Id;
        poolId.Should().NotBeNullOrWhiteSpace();

        var list = await client.GetFromJsonAsync<UserPoolListResponse>(
            "/api/services/cognito/user-pools", cancellationToken);
        list.Should().NotBeNull();
        list!.UserPools.Should().ContainSingle(pool => pool.Id == poolId);

        var detail = await client.GetFromJsonAsync<UserPoolDetailResponse>(
            $"/api/services/cognito/user-pools/{Uri.EscapeDataString(poolId)}", cancellationToken);
        detail.Should().NotBeNull();
        detail!.Id.Should().Be(poolId);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/cognito/user-pools/{Uri.EscapeDataString(poolId)}", cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
