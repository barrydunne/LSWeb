using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Diagnostics;
using Foundation.Application.SecretsManager;
using Foundation.Domain.Configuration;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetSecretValue;

internal sealed partial class GetSecretValueQueryHandler : IQueryHandler<GetSecretValueQuery, GetSecretValueQueryResult>
{
    private readonly ISecretsManagerClient _client;
    private readonly IRedactionService _redaction;
    private readonly ILogger _logger;

    public GetSecretValueQueryHandler(ISecretsManagerClient client, IRedactionService redaction, ILogger<GetSecretValueQueryHandler> logger)
    {
        _client = client;
        _redaction = redaction;
        _logger = logger;
    }

    public async Task<Result<GetSecretValueQueryResult>> Handle(GetSecretValueQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.SecretId, request.Reveal);
        var secret = await _client.GetSecretValueAsync(request.SecretId, cancellationToken);
        LogHandled(secret.IsSuccess);

        if (!secret.IsSuccess)
        {
            Result<GetSecretValueQueryResult> failure = secret.Error!.Value;
            return failure;
        }

        var value = _redaction.Resolve(
            new ConfigValue(secret.Value.Name, secret.Value.SecretString, ConfigSource.Default, IsSensitive: true),
            request.Reveal);

        return new GetSecretValueQueryResult(
            secret.Value.Name,
            secret.Value.Arn,
            secret.Value.VersionId,
            value,
            _redaction.CanReveal);
    }

    [LoggerMessage(LogLevel.Trace, "Getting Secrets Manager secret value for {SecretId}. Reveal: {Reveal}")]
    private partial void LogHandling(string secretId, bool reveal);

    [LoggerMessage(LogLevel.Trace, "Secrets Manager secret value retrieval handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
