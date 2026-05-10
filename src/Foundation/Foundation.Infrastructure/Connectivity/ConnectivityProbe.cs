using System.Diagnostics.CodeAnalysis;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Foundation.Application.Connectivity;
using Foundation.Infrastructure.Aws;
using Microsoft.Extensions.Logging;

namespace Foundation.Infrastructure.Connectivity;

/// <summary>
/// Probes backend reachability with a cheap, standard AWS call so the same check
/// works against LocalStack or real AWS. Any failure is reported as unreachable
/// rather than thrown, keeping the surface non-crashing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed partial class ConnectivityProbe : IConnectivityProbe
{
    private readonly IAwsClientFactory _clientFactory;
    private readonly ILogger _logger;

    public ConnectivityProbe(IAwsClientFactory clientFactory, ILogger<ConnectivityProbe> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    public async Task<ConnectivityProbeResult> CheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = _clientFactory.CreateClient<AmazonSecurityTokenServiceClient>();
            _ = await client.GetCallerIdentityAsync(new GetCallerIdentityRequest(), cancellationToken);
            return new ConnectivityProbeResult(true, null);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogProbeFailed(exception);
            return new ConnectivityProbeResult(false, exception.Message);
        }
    }

    [LoggerMessage(LogLevel.Warning, "AWS connectivity probe failed.")]
    private partial void LogProbeFailed(Exception exception);
}
