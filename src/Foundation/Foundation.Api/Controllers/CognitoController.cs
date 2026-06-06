using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateUserPool;
using Foundation.Application.Commands.CreateUserPoolClient;
using Foundation.Application.Commands.DeleteUserPool;
using Foundation.Application.Commands.DeleteUserPoolClient;
using Foundation.Application.Commands.UpdateUserPoolClient;
using Foundation.Application.Queries.GetUserPool;
using Foundation.Application.Queries.GetUserPoolClient;
using Foundation.Application.Queries.ListUserPoolClients;
using Foundation.Application.Queries.ListUserPools;
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
                userPool.UserPool.LastModifiedDate)),
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
                request.AutoVerifiedAttributes ?? []),
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
}
