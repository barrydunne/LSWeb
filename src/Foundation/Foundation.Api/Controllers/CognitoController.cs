using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateCognitoUser;
using Foundation.Application.Commands.CreateUserPool;
using Foundation.Application.Commands.CreateUserPoolClient;
using Foundation.Application.Commands.DeleteCognitoUser;
using Foundation.Application.Commands.DeleteUserPool;
using Foundation.Application.Commands.DeleteUserPoolClient;
using Foundation.Application.Commands.RegenerateUserPoolClientSecret;
using Foundation.Application.Commands.SetCognitoUserEnabled;
using Foundation.Application.Commands.SetCognitoUserPassword;
using Foundation.Application.Commands.UpdateUserPoolClient;
using Foundation.Application.Queries.GetUser;
using Foundation.Application.Queries.GetUserPool;
using Foundation.Application.Queries.GetUserPoolClient;
using Foundation.Application.Queries.ListUserPoolClients;
using Foundation.Application.Queries.ListUserPools;
using Foundation.Application.Queries.ListUsers;
using Foundation.Application.Queries.RequestCognitoToken;
using Foundation.Domain.Cognito;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to Amazon Cognito user pools: listing the available pools, viewing the details
/// of a single pool, and creating or deleting pools.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/cognito")]
public partial class CognitoController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CognitoController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public CognitoController(ISender sender, ILogger<CognitoController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the Amazon Cognito user pools available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the user pool summaries.</returns>
    [HttpGet("user-pools")]
    [ProducesResponseType(typeof(UserPoolListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListUserPools(CancellationToken cancellationToken)
    {
        LogHandlingListUserPools();
        var result = await _sender.Send(new ListUserPoolsQuery(), cancellationToken);
        LogListUserPoolsHandled(result.IsSuccess);
        return result.Match(
            userPools => Results.Ok(new UserPoolListResponse(
                userPools.UserPools
                    .Select(userPool => new UserPoolSummaryResponse(
                        userPool.Id,
                        userPool.Name,
                        userPool.CreationDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the details of a single Amazon Cognito user pool by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user pool to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the user pool details.</returns>
    [HttpGet("user-pools/{id}")]
    [ProducesResponseType(typeof(UserPoolDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetUserPool(string id, CancellationToken cancellationToken)
    {
        LogHandlingGetUserPool(id);
        var result = await _sender.Send(new GetUserPoolQuery(id), cancellationToken);
        LogGetUserPoolHandled(result.IsSuccess);
        return result.Match(
            userPool => Results.Ok(new UserPoolDetailResponse(
                userPool.UserPool.Id,
                userPool.UserPool.Name,
                userPool.UserPool.Arn,
                userPool.UserPool.MfaConfiguration,
                userPool.UserPool.EstimatedNumberOfUsers,
                userPool.UserPool.UsernameAttributes,
                userPool.UserPool.AutoVerifiedAttributes,
                userPool.UserPool.CreationDate,
                userPool.UserPool.LastModifiedDate,
                ToPasswordPolicyModel(userPool.UserPool.PasswordPolicy))),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new Amazon Cognito user pool.
    /// </summary>
    /// <param name="request">The user pool configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new user pool id.</returns>
    [HttpPost("user-pools")]
    [ProducesResponseType(typeof(UserPoolCreatedResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateUserPool(
        [FromBody] UserPoolCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateUserPool(request.Name);
        var result = await _sender.Send(
            new CreateUserPoolCommand(
                request.Name,
                request.MfaConfiguration,
                request.UsernameAttributes ?? [],
                request.AutoVerifiedAttributes ?? [],
                ToPasswordPolicy(request.PasswordPolicy)),
            cancellationToken);
        LogCreateUserPoolHandled(result.IsSuccess);
        return result.Match(
            id => Results.Created(
                $"/api/services/cognito/user-pools/{Uri.EscapeDataString(id)}",
                new UserPoolCreatedResponse(id)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an Amazon Cognito user pool by its identifier. This action cannot be undone.
    /// </summary>
    /// <param name="id">The unique identifier of the user pool to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("user-pools/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteUserPool(string id, CancellationToken cancellationToken)
    {
        LogHandlingDeleteUserPool(id);
        var result = await _sender.Send(new DeleteUserPoolCommand(id), cancellationToken);
        LogDeleteUserPoolHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the app clients configured within an Amazon Cognito user pool.
    /// </summary>
    /// <param name="poolId">The identifier of the user pool whose app clients should be listed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the app client summaries.</returns>
    [HttpGet("user-pools/{poolId}/clients")]
    [ProducesResponseType(typeof(UserPoolClientListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListUserPoolClients(string poolId, CancellationToken cancellationToken)
    {
        LogHandlingListUserPoolClients(poolId);
        var result = await _sender.Send(new ListUserPoolClientsQuery(poolId), cancellationToken);
        LogListUserPoolClientsHandled(result.IsSuccess);
        return result.Match(
            clients => Results.Ok(new UserPoolClientListResponse(
                clients.Clients
                    .Select(client => new UserPoolClientSummaryResponse(
                        client.ClientId,
                        client.ClientName,
                        client.UserPoolId))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the details of a single Amazon Cognito user pool app client, including its client secret.
    /// </summary>
    /// <param name="poolId">The identifier of the user pool the app client belongs to.</param>
    /// <param name="clientId">The unique identifier of the app client to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the app client details.</returns>
    [HttpGet("user-pools/{poolId}/clients/{clientId}")]
    [ProducesResponseType(typeof(UserPoolClientDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetUserPoolClient(string poolId, string clientId, CancellationToken cancellationToken)
    {
        LogHandlingGetUserPoolClient(poolId, clientId);
        var result = await _sender.Send(new GetUserPoolClientQuery(poolId, clientId), cancellationToken);
        LogGetUserPoolClientHandled(result.IsSuccess);
        return result.Match(
            client => Results.Ok(ToDetailResponse(client.Client)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new app client within an Amazon Cognito user pool.
    /// </summary>
    /// <param name="poolId">The identifier of the user pool to create the app client in.</param>
    /// <param name="request">The app client configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new app client details.</returns>
    [HttpPost("user-pools/{poolId}/clients")]
    [ProducesResponseType(typeof(UserPoolClientDetailResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateUserPoolClient(
        string poolId, [FromBody] UserPoolClientCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateUserPoolClient(request.ClientName);
        var result = await _sender.Send(
            new CreateUserPoolClientCommand(
                poolId,
                request.ClientName,
                request.GenerateSecret,
                request.ExplicitAuthFlows ?? [],
                request.AllowedOAuthFlows ?? [],
                request.AllowedOAuthScopes ?? [],
                request.CallbackURLs ?? [],
                request.AllowedOAuthFlowsUserPoolClient),
            cancellationToken);
        LogCreateUserPoolClientHandled(result.IsSuccess);
        return result.Match(
            client => Results.Created(
                $"/api/services/cognito/user-pools/{Uri.EscapeDataString(poolId)}/clients/{Uri.EscapeDataString(client.ClientId)}",
                ToDetailResponse(client)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates the configuration of an existing Amazon Cognito user pool app client.
    /// </summary>
    /// <param name="poolId">The identifier of the user pool the app client belongs to.</param>
    /// <param name="clientId">The unique identifier of the app client to update.</param>
    /// <param name="request">The app client configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("user-pools/{poolId}/clients/{clientId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateUserPoolClient(
        string poolId, string clientId, [FromBody] UserPoolClientUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateUserPoolClient(clientId);
        var result = await _sender.Send(
            new UpdateUserPoolClientCommand(
                poolId,
                clientId,
                request.ClientName,
                request.ExplicitAuthFlows ?? [],
                request.AllowedOAuthFlows ?? [],
                request.AllowedOAuthScopes ?? [],
                request.CallbackURLs ?? [],
                request.AllowedOAuthFlowsUserPoolClient),
            cancellationToken);
        LogUpdateUserPoolClientHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an app client from an Amazon Cognito user pool. This action cannot be undone.
    /// </summary>
    /// <param name="poolId">The identifier of the user pool the app client belongs to.</param>
    /// <param name="clientId">The unique identifier of the app client to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("user-pools/{poolId}/clients/{clientId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteUserPoolClient(string poolId, string clientId, CancellationToken cancellationToken)
    {
        LogHandlingDeleteUserPoolClient(clientId);
        var result = await _sender.Send(new DeleteUserPoolClientCommand(poolId, clientId), cancellationToken);
        LogDeleteUserPoolClientHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Regenerates the client secret of an Amazon Cognito user pool app client by recreating it.
    /// The recreated client has a new identifier and a freshly generated secret.
    /// </summary>
    /// <param name="poolId">The identifier of the user pool the app client belongs to.</param>
    /// <param name="clientId">The unique identifier of the app client whose secret should be regenerated.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the recreated app client details with the new secret.</returns>
    [HttpPost("user-pools/{poolId}/clients/{clientId}/regenerate-secret")]
    [ProducesResponseType(typeof(UserPoolClientDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> RegenerateUserPoolClientSecret(string poolId, string clientId, CancellationToken cancellationToken)
    {
        LogHandlingRegenerateUserPoolClientSecret(clientId);
        var result = await _sender.Send(new RegenerateUserPoolClientSecretCommand(poolId, clientId), cancellationToken);
        LogRegenerateUserPoolClientSecretHandled(result.IsSuccess);
        return result.Match(
            client => Results.Ok(ToDetailResponse(client)),
            error => error.AsHttpResult());
    }

    private static UserPoolClientDetailResponse ToDetailResponse(Foundation.Domain.Cognito.UserPoolClientDetail client)
        => new(
            client.ClientId,
            client.ClientName,
            client.UserPoolId,
            client.ClientSecret,
            client.GenerateSecret,
            client.ExplicitAuthFlows,
            client.AllowedOAuthFlows,
            client.AllowedOAuthScopes,
            client.CallbackURLs,
            client.AllowedOAuthFlowsUserPoolClient,
            client.CreationDate,
            client.LastModifiedDate);

    /// <summary>
    /// Lists the users within an Amazon Cognito user pool.
    /// </summary>
    /// <param name="poolId">The identifier of the user pool whose users should be listed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the user summaries.</returns>
    [HttpGet("user-pools/{poolId}/users")]
    [ProducesResponseType(typeof(CognitoUserListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListUsers(string poolId, CancellationToken cancellationToken)
    {
        LogHandlingListUsers(poolId);
        var result = await _sender.Send(new ListUsersQuery(poolId), cancellationToken);
        LogListUsersHandled(result.IsSuccess);
        return result.Match(
            users => Results.Ok(new CognitoUserListResponse(
                users.Users
                    .Select(user => new CognitoUserSummaryResponse(
                        user.Username,
                        user.Status,
                        user.Enabled,
                        user.CreatedDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the details of a single Amazon Cognito user by its username.
    /// </summary>
    /// <param name="poolId">The identifier of the user pool the user belongs to.</param>
    /// <param name="username">The unique username of the user to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the user details.</returns>
    [HttpGet("user-pools/{poolId}/users/{username}")]
    [ProducesResponseType(typeof(CognitoUserDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetUser(string poolId, string username, CancellationToken cancellationToken)
    {
        LogHandlingGetUser(poolId, username);
        var result = await _sender.Send(new GetUserQuery(poolId, username), cancellationToken);
        LogGetUserHandled(result.IsSuccess);
        return result.Match(
            user => Results.Ok(ToUserDetailResponse(user.User)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new user within an Amazon Cognito user pool.
    /// </summary>
    /// <param name="poolId">The identifier of the user pool to create the user in.</param>
    /// <param name="request">The user configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new user details.</returns>
    [HttpPost("user-pools/{poolId}/users")]
    [ProducesResponseType(typeof(CognitoUserDetailResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateUser(
        string poolId, [FromBody] CognitoUserCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateUser(request.Username);
        var result = await _sender.Send(
            new CreateCognitoUserCommand(
                poolId,
                request.Username,
                (request.Attributes ?? [])
                    .Select(attribute => new CognitoUserAttributeEntry(attribute.Name, attribute.Value))
                    .ToList(),
                request.TemporaryPassword),
            cancellationToken);
        LogCreateUserHandled(result.IsSuccess);
        return result.Match(
            user => Results.Created(
                $"/api/services/cognito/user-pools/{Uri.EscapeDataString(poolId)}/users/{Uri.EscapeDataString(user.Username)}",
                ToUserDetailResponse(user)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a user from an Amazon Cognito user pool. This action cannot be undone.
    /// </summary>
    /// <param name="poolId">The identifier of the user pool the user belongs to.</param>
    /// <param name="username">The unique username of the user to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("user-pools/{poolId}/users/{username}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteUser(string poolId, string username, CancellationToken cancellationToken)
    {
        LogHandlingDeleteUser(username);
        var result = await _sender.Send(new DeleteCognitoUserCommand(poolId, username), cancellationToken);
        LogDeleteUserHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Sets the password of an Amazon Cognito user.
    /// </summary>
    /// <param name="poolId">The identifier of the user pool the user belongs to.</param>
    /// <param name="username">The unique username of the user.</param>
    /// <param name="request">The password configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("user-pools/{poolId}/users/{username}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> SetUserPassword(
        string poolId, string username, [FromBody] CognitoUserPasswordRequest request, CancellationToken cancellationToken)
    {
        LogHandlingSetUserPassword(username);
        var result = await _sender.Send(
            new SetCognitoUserPasswordCommand(poolId, username, request.Password, request.Permanent),
            cancellationToken);
        LogSetUserPasswordHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Enables or disables an Amazon Cognito user account.
    /// </summary>
    /// <param name="poolId">The identifier of the user pool the user belongs to.</param>
    /// <param name="username">The unique username of the user.</param>
    /// <param name="request">The enabled state to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("user-pools/{poolId}/users/{username}/enabled")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> SetUserEnabled(
        string poolId, string username, [FromBody] CognitoUserEnabledRequest request, CancellationToken cancellationToken)
    {
        LogHandlingSetUserEnabled(username);
        var result = await _sender.Send(
            new SetCognitoUserEnabledCommand(poolId, username, request.Enabled),
            cancellationToken);
        LogSetUserEnabledHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Requests bearer tokens for an Amazon Cognito app client and decodes the identity token claims.
    /// </summary>
    /// <param name="poolId">The identifier of the user pool the app client belongs to.</param>
    /// <param name="clientId">The identifier of the app client to authenticate against.</param>
    /// <param name="request">The credentials to authenticate with.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the issued tokens and decoded claims.</returns>
    [HttpPost("user-pools/{poolId}/clients/{clientId}/token")]
    [ProducesResponseType(typeof(CognitoTokenResponse), StatusCodes.Status200OK)]
    public async Task<IResult> RequestToken(
        string poolId, string clientId, [FromBody] CognitoTokenRequest request, CancellationToken cancellationToken)
    {
        LogHandlingRequestToken(clientId, request.Username);
        var result = await _sender.Send(
            new RequestCognitoTokenQuery(poolId, clientId, request.Username, request.Password),
            cancellationToken);
        LogRequestTokenHandled(result.IsSuccess);
        return result.Match(
            token => Results.Ok(new CognitoTokenResponse(
                token.Token.AccessToken,
                token.Token.IdToken,
                token.Token.RefreshToken,
                token.Token.TokenType,
                token.Token.ExpiresIn,
                token.Token.Claims
                    .Select(claim => new CognitoUserAttributeResponse(claim.Name, claim.Value))
                    .ToList())),
            error => error.AsHttpResult());
    }

    private static CognitoUserDetailResponse ToUserDetailResponse(CognitoUserDetail user)
        => new(
            user.Username,
            user.Status,
            user.Enabled,
            user.Attributes
                .Select(attribute => new CognitoUserAttributeResponse(attribute.Name, attribute.Value))
                .ToList(),
            user.CreatedDate,
            user.LastModifiedDate);

    private static PasswordPolicyModel? ToPasswordPolicyModel(PasswordPolicy? policy)
        => policy is null
            ? null
            : new PasswordPolicyModel(
                policy.MinimumLength,
                policy.RequireUppercase,
                policy.RequireLowercase,
                policy.RequireNumbers,
                policy.RequireSymbols);

    private static PasswordPolicy? ToPasswordPolicy(PasswordPolicyModel? policy)
        => policy is null
            ? null
            : new PasswordPolicy(
                policy.MinimumLength,
                policy.RequireUppercase,
                policy.RequireLowercase,
                policy.RequireNumbers,
                policy.RequireSymbols);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito user pool list request.")]
    private partial void LogHandlingListUserPools();

    [LoggerMessage(LogLevel.Trace, "Cognito user pool list request handled. Success: {Success}")]
    private partial void LogListUserPoolsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito user pool get request for {Id}.")]
    private partial void LogHandlingGetUserPool(string id);

    [LoggerMessage(LogLevel.Trace, "Cognito user pool get request handled. Success: {Success}")]
    private partial void LogGetUserPoolHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito user pool create request for {Name}.")]
    private partial void LogHandlingCreateUserPool(string name);

    [LoggerMessage(LogLevel.Trace, "Cognito user pool create request handled. Success: {Success}")]
    private partial void LogCreateUserPoolHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito user pool delete request for {Id}.")]
    private partial void LogHandlingDeleteUserPool(string id);

    [LoggerMessage(LogLevel.Trace, "Cognito user pool delete request handled. Success: {Success}")]
    private partial void LogDeleteUserPoolHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito app client list request for {PoolId}.")]
    private partial void LogHandlingListUserPoolClients(string poolId);

    [LoggerMessage(LogLevel.Trace, "Cognito app client list request handled. Success: {Success}")]
    private partial void LogListUserPoolClientsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito app client get request for {PoolId}/{ClientId}.")]
    private partial void LogHandlingGetUserPoolClient(string poolId, string clientId);

    [LoggerMessage(LogLevel.Trace, "Cognito app client get request handled. Success: {Success}")]
    private partial void LogGetUserPoolClientHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito app client create request for {ClientName}.")]
    private partial void LogHandlingCreateUserPoolClient(string clientName);

    [LoggerMessage(LogLevel.Trace, "Cognito app client create request handled. Success: {Success}")]
    private partial void LogCreateUserPoolClientHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito app client update request for {ClientId}.")]
    private partial void LogHandlingUpdateUserPoolClient(string clientId);

    [LoggerMessage(LogLevel.Trace, "Cognito app client update request handled. Success: {Success}")]
    private partial void LogUpdateUserPoolClientHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito app client delete request for {ClientId}.")]
    private partial void LogHandlingDeleteUserPoolClient(string clientId);

    [LoggerMessage(LogLevel.Trace, "Cognito app client delete request handled. Success: {Success}")]
    private partial void LogDeleteUserPoolClientHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito app client regenerate secret request for {ClientId}.")]
    private partial void LogHandlingRegenerateUserPoolClientSecret(string clientId);

    [LoggerMessage(LogLevel.Trace, "Cognito app client regenerate secret request handled. Success: {Success}")]
    private partial void LogRegenerateUserPoolClientSecretHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito user list request for {PoolId}.")]
    private partial void LogHandlingListUsers(string poolId);

    [LoggerMessage(LogLevel.Trace, "Cognito user list request handled. Success: {Success}")]
    private partial void LogListUsersHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito user get request for {PoolId}/{Username}.")]
    private partial void LogHandlingGetUser(string poolId, string username);

    [LoggerMessage(LogLevel.Trace, "Cognito user get request handled. Success: {Success}")]
    private partial void LogGetUserHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito user create request for {Username}.")]
    private partial void LogHandlingCreateUser(string username);

    [LoggerMessage(LogLevel.Trace, "Cognito user create request handled. Success: {Success}")]
    private partial void LogCreateUserHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito user delete request for {Username}.")]
    private partial void LogHandlingDeleteUser(string username);

    [LoggerMessage(LogLevel.Trace, "Cognito user delete request handled. Success: {Success}")]
    private partial void LogDeleteUserHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito user password request for {Username}.")]
    private partial void LogHandlingSetUserPassword(string username);

    [LoggerMessage(LogLevel.Trace, "Cognito user password request handled. Success: {Success}")]
    private partial void LogSetUserPasswordHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito user enabled request for {Username}.")]
    private partial void LogHandlingSetUserEnabled(string username);

    [LoggerMessage(LogLevel.Trace, "Cognito user enabled request handled. Success: {Success}")]
    private partial void LogSetUserEnabledHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Cognito token request for client {ClientId} and user {Username}.")]
    private partial void LogHandlingRequestToken(string clientId, string username);

    [LoggerMessage(LogLevel.Trace, "Cognito token request handled. Success: {Success}")]
    private partial void LogRequestTokenHandled(bool success);
}
