using System.Diagnostics.CodeAnalysis;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Cognito;
using Foundation.Domain.Cognito;
using Foundation.Infrastructure.Aws;

namespace Foundation.Infrastructure.Cognito;

/// <summary>
/// Reads Amazon Cognito user pools through the resilient AWS gateway so the same code works against
/// LocalStack or real AWS. All access flows through <see cref="IAwsGateway"/>, which records capability
/// and converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class CognitoClientAdapter : ICognitoClient
{
    private const string ServiceKey = "cognito";
    private const int PageSize = 60;

    private readonly IAwsGateway _gateway;

    public CognitoClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<UserPoolSummary>>> ListUserPoolsAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCognitoIdentityProviderClient, IReadOnlyList<UserPoolSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var userPools = new List<UserPoolSummary>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListUserPoolsAsync(
                        new ListUserPoolsRequest { MaxResults = PageSize, NextToken = nextToken },
                        token);

                    foreach (var userPool in response.UserPools ?? [])
                        userPools.Add(ToSummary(userPool));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return userPools;
            },
            cancellationToken);

    public Task<Result<UserPoolDetail>> GetUserPoolAsync(
        string id, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCognitoIdentityProviderClient, UserPoolDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.DescribeUserPoolAsync(
                    new DescribeUserPoolRequest { UserPoolId = id },
                    token);

                var userPool = response.UserPool;
                return new UserPoolDetail(
                    userPool.Id ?? string.Empty,
                    userPool.Name ?? string.Empty,
                    string.IsNullOrEmpty(userPool.Arn) ? null : userPool.Arn,
                    userPool.MfaConfiguration?.Value,
                    userPool.EstimatedNumberOfUsers,
                    (userPool.UsernameAttributes ?? []).ToList(),
                    (userPool.AutoVerifiedAttributes ?? []).ToList(),
                    ToTimestamp(userPool.CreationDate),
                    ToTimestamp(userPool.LastModifiedDate));
            },
            cancellationToken);

    public async Task<Result<string>> CreateUserPoolAsync(
        UserPoolSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonCognitoIdentityProviderClient, string>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new CreateUserPoolRequest
                {
                    PoolName = specification.Name,
                    UsernameAttributes = specification.UsernameAttributes.ToList(),
                    AutoVerifiedAttributes = specification.AutoVerifiedAttributes.ToList(),
                };

                if (!string.IsNullOrEmpty(specification.MfaConfiguration))
                    request.MfaConfiguration = UserPoolMfaType.FindValue(specification.MfaConfiguration);

                var response = await client.CreateUserPoolAsync(request, token);
                return response.UserPool.Id ?? string.Empty;
            },
            cancellationToken);

        return result.IsSuccess ? result.Value : result.Error!.Value;
    }

    public async Task<Result> DeleteUserPoolAsync(
        string id, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonCognitoIdentityProviderClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteUserPoolAsync(
                    new DeleteUserPoolRequest { UserPoolId = id },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<UserPoolClientSummary>>> ListUserPoolClientsAsync(
        string userPoolId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCognitoIdentityProviderClient, IReadOnlyList<UserPoolClientSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var clients = new List<UserPoolClientSummary>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListUserPoolClientsAsync(
                        new ListUserPoolClientsRequest { UserPoolId = userPoolId, MaxResults = PageSize, NextToken = nextToken },
                        token);

                    foreach (var appClient in response.UserPoolClients ?? [])
                        clients.Add(new UserPoolClientSummary(
                            appClient.ClientId ?? string.Empty,
                            appClient.ClientName ?? string.Empty,
                            appClient.UserPoolId ?? string.Empty));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return clients;
            },
            cancellationToken);

    public Task<Result<UserPoolClientDetail>> GetUserPoolClientAsync(
        string userPoolId, string clientId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCognitoIdentityProviderClient, UserPoolClientDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.DescribeUserPoolClientAsync(
                    new DescribeUserPoolClientRequest { UserPoolId = userPoolId, ClientId = clientId },
                    token);

                return ToClientDetail(response.UserPoolClient);
            },
            cancellationToken);

    public async Task<Result<UserPoolClientDetail>> CreateUserPoolClientAsync(
        UserPoolClientSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonCognitoIdentityProviderClient, UserPoolClientDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new CreateUserPoolClientRequest
                {
                    UserPoolId = specification.UserPoolId,
                    ClientName = specification.ClientName,
                    GenerateSecret = specification.GenerateSecret,
                    ExplicitAuthFlows = specification.ExplicitAuthFlows.ToList(),
                    AllowedOAuthFlows = specification.AllowedOAuthFlows.ToList(),
                    AllowedOAuthScopes = specification.AllowedOAuthScopes.ToList(),
                    CallbackURLs = specification.CallbackURLs.ToList(),
                    AllowedOAuthFlowsUserPoolClient = specification.AllowedOAuthFlowsUserPoolClient,
                };

                var response = await client.CreateUserPoolClientAsync(request, token);
                return ToClientDetail(response.UserPoolClient);
            },
            cancellationToken);

        return result.IsSuccess ? result.Value : result.Error!.Value;
    }

    public async Task<Result> UpdateUserPoolClientAsync(
        UserPoolClientSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonCognitoIdentityProviderClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new UpdateUserPoolClientRequest
                {
                    UserPoolId = specification.UserPoolId,
                    ClientId = specification.ClientId,
                    ClientName = specification.ClientName,
                    ExplicitAuthFlows = specification.ExplicitAuthFlows.ToList(),
                    AllowedOAuthFlows = specification.AllowedOAuthFlows.ToList(),
                    AllowedOAuthScopes = specification.AllowedOAuthScopes.ToList(),
                    CallbackURLs = specification.CallbackURLs.ToList(),
                    AllowedOAuthFlowsUserPoolClient = specification.AllowedOAuthFlowsUserPoolClient,
                };

                await client.UpdateUserPoolClientAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteUserPoolClientAsync(
        string userPoolId, string clientId, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonCognitoIdentityProviderClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteUserPoolClientAsync(
                    new DeleteUserPoolClientRequest { UserPoolId = userPoolId, ClientId = clientId },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    private static UserPoolClientDetail ToClientDetail(UserPoolClientType client)
        => new(
            client.ClientId ?? string.Empty,
            client.ClientName ?? string.Empty,
            client.UserPoolId ?? string.Empty,
            string.IsNullOrEmpty(client.ClientSecret) ? null : client.ClientSecret,
            !string.IsNullOrEmpty(client.ClientSecret),
            (client.ExplicitAuthFlows ?? []).ToList(),
            (client.AllowedOAuthFlows ?? []).ToList(),
            (client.AllowedOAuthScopes ?? []).ToList(),
            (client.CallbackURLs ?? []).ToList(),
            client.AllowedOAuthFlowsUserPoolClient ?? false,
            ToTimestamp(client.CreationDate),
            ToTimestamp(client.LastModifiedDate));

    private static UserPoolSummary ToSummary(UserPoolDescriptionType userPool)
        => new(
            userPool.Id ?? string.Empty,
            userPool.Name ?? string.Empty,
            ToTimestamp(userPool.CreationDate));

    private static DateTimeOffset? ToTimestamp(DateTime? value)
        => value is null
            ? null
            : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
}
