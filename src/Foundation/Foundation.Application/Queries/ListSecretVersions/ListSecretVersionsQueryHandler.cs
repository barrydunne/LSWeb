using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.SecretsManager;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListSecretVersions;

internal sealed partial class ListSecretVersionsQueryHandler
    : IQueryHandler<ListSecretVersionsQuery, ListSecretVersionsQueryResult>
{
    private readonly ISecretsManagerClient _client;
    private readonly ILogger _logger;

    public ListSecretVersionsQueryHandler(
        ISecretsManagerClient client, ILogger<ListSecretVersionsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListSecretVersionsQueryResult>> Handle(
        ListSecretVersionsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.SecretId);
        var versions = await _client.ListSecretVersionsAsync(request.SecretId, cancellationToken);
        LogHandled(versions.IsSuccess);

        if (!versions.IsSuccess)
        {
            Result<ListSecretVersionsQueryResult> failure = versions.Error!.Value;
            return failure;
        }

        return new ListSecretVersionsQueryResult(
            versions.Value.Name,
            versions.Value.Arn,
            versions.Value.Versions);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Secrets Manager secret versions for {SecretId}.")]
    private partial void LogHandling(string secretId);

    [LoggerMessage(LogLevel.Trace, "Secrets Manager secret version listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
