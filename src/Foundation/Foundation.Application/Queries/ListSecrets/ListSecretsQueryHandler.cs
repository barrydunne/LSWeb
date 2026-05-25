using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.SecretsManager;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListSecrets;

internal sealed partial class ListSecretsQueryHandler
    : IQueryHandler<ListSecretsQuery, ListSecretsQueryResult>
{
    private readonly ISecretsManagerClient _client;
    private readonly ILogger _logger;

    public ListSecretsQueryHandler(ISecretsManagerClient client, ILogger<ListSecretsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListSecretsQueryResult>> Handle(
        ListSecretsQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var secrets = await _client.ListSecretsAsync(cancellationToken);
        LogHandled(secrets.IsSuccess);

        if (!secrets.IsSuccess)
        {
            Result<ListSecretsQueryResult> failure = secrets.Error!.Value;
            return failure;
        }

        return new ListSecretsQueryResult(secrets.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Secrets Manager secrets.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "Secrets Manager secret listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
