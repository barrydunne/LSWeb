using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateSecret;
using Foundation.Application.Commands.DeleteSecret;
using Foundation.Application.Commands.PutSecretValue;
using Foundation.Application.Queries.GetSecretValue;
using Foundation.Application.Queries.ListSecrets;
using Foundation.Application.Queries.ListSecretVersions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to AWS Secrets Manager: listing the secrets on the configured backend, creating a
/// new secret, and deleting an existing one.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/secrets-manager")]
public partial class SecretsManagerController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretsManagerController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public SecretsManagerController(ISender sender, ILogger<SecretsManagerController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the Secrets Manager secrets available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the secret summaries.</returns>
    [HttpGet("secrets")]
    [ProducesResponseType(typeof(SecretListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListSecrets(CancellationToken cancellationToken)
    {
        LogHandlingListSecrets();
        var result = await _sender.Send(new ListSecretsQuery(), cancellationToken);
        LogListSecretsHandled(result.IsSuccess);
        return result.Match(
            secrets => Results.Ok(new SecretListResponse(
                secrets.Secrets
                    .Select(secret => new SecretSummaryResponse(
                        secret.Name,
                        secret.Arn,
                        secret.Description,
                        secret.CreatedDate,
                        secret.LastChangedDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new Secrets Manager secret with the supplied name, description, and value.
    /// </summary>
    /// <param name="request">The secret to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created secret.</returns>
    [HttpPost("secrets")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateSecret(
        [FromBody] SecretCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateSecret(request.Name);
        var result = await _sender.Send(
            new CreateSecretCommand(request.Name, request.Description, request.SecretString),
            cancellationToken);
        LogCreateSecretHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/secrets-manager/secrets/{Uri.EscapeDataString(request.Name)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a Secrets Manager secret and all of the versions it contains. This is a destructive
    /// action that cannot be undone.
    /// </summary>
    /// <param name="secretId">The name or ARN of the secret to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("secrets/{secretId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteSecret(string secretId, CancellationToken cancellationToken)
    {
        LogHandlingDeleteSecret(secretId);
        var result = await _sender.Send(new DeleteSecretCommand(secretId), cancellationToken);
        LogDeleteSecretHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the current value of a Secrets Manager secret, masking the value unless a reveal is both
    /// requested and permitted by the host.
    /// </summary>
    /// <param name="secretId">The name or ARN of the secret to read.</param>
    /// <param name="reveal">Whether to reveal the sensitive value, subject to host policy.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the secret value.</returns>
    [HttpGet("secrets/{secretId}/value")]
    [ProducesResponseType(typeof(SecretValueResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetSecretValue(
        string secretId, [FromQuery] bool reveal, CancellationToken cancellationToken)
    {
        LogHandlingGetSecretValue(secretId, reveal);
        var result = await _sender.Send(new GetSecretValueQuery(secretId, reveal), cancellationToken);
        LogGetSecretValueHandled(result.IsSuccess);
        return result.Match(
            value => Results.Ok(new SecretValueResponse(
                value.Name,
                value.Arn,
                value.VersionId,
                value.Value,
                value.RevealAllowed)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Stores a new value against an existing Secrets Manager secret, creating a new version.
    /// </summary>
    /// <param name="secretId">The name or ARN of the secret to update.</param>
    /// <param name="request">The secret value to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("secrets/{secretId}/value")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateSecretValue(
        string secretId, [FromBody] SecretValueUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateSecretValue(secretId);
        var result = await _sender.Send(
            new PutSecretValueCommand(secretId, request.SecretString), cancellationToken);
        LogUpdateSecretValueHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the versions held for a Secrets Manager secret along with the staging labels attached to
    /// each version, such as AWSCURRENT and AWSPREVIOUS.
    /// </summary>
    /// <param name="secretId">The name or ARN of the secret to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the secret versions.</returns>
    [HttpGet("secrets/{secretId}/versions")]
    [ProducesResponseType(typeof(SecretVersionListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListSecretVersions(string secretId, CancellationToken cancellationToken)
    {
        LogHandlingListSecretVersions(secretId);
        var result = await _sender.Send(new ListSecretVersionsQuery(secretId), cancellationToken);
        LogListSecretVersionsHandled(result.IsSuccess);
        return result.Match(
            versions => Results.Ok(new SecretVersionListResponse(
                versions.Name,
                versions.Arn,
                versions.Versions
                    .Select(version => new SecretVersionResponse(
                        version.VersionId,
                        version.Stages,
                        version.CreatedDate,
                        version.LastAccessedDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling Secrets Manager secret list request.")]
    private partial void LogHandlingListSecrets();

    [LoggerMessage(LogLevel.Trace, "Secrets Manager secret list request handled. Success: {Success}")]
    private partial void LogListSecretsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Secrets Manager secret create request for {Name}.")]
    private partial void LogHandlingCreateSecret(string name);

    [LoggerMessage(LogLevel.Trace, "Secrets Manager secret create request handled. Success: {Success}")]
    private partial void LogCreateSecretHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Secrets Manager secret delete request for {SecretId}.")]
    private partial void LogHandlingDeleteSecret(string secretId);

    [LoggerMessage(LogLevel.Trace, "Secrets Manager secret delete request handled. Success: {Success}")]
    private partial void LogDeleteSecretHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Secrets Manager secret value request for {SecretId}. Reveal: {Reveal}")]
    private partial void LogHandlingGetSecretValue(string secretId, bool reveal);

    [LoggerMessage(LogLevel.Trace, "Secrets Manager secret value request handled. Success: {Success}")]
    private partial void LogGetSecretValueHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Secrets Manager secret value update request for {SecretId}.")]
    private partial void LogHandlingUpdateSecretValue(string secretId);

    [LoggerMessage(LogLevel.Trace, "Secrets Manager secret value update request handled. Success: {Success}")]
    private partial void LogUpdateSecretValueHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Secrets Manager secret versions request for {SecretId}.")]
    private partial void LogHandlingListSecretVersions(string secretId);

    [LoggerMessage(LogLevel.Trace, "Secrets Manager secret versions request handled. Success: {Success}")]
    private partial void LogListSecretVersionsHandled(bool success);
}
