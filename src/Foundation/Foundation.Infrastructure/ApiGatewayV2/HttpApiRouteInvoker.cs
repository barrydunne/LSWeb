using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Infrastructure.ApiGatewayV2;

/// <summary>
/// Invokes an Amazon API Gateway v2 route over HTTP so its authorization behaviour can be observed
/// directly from the UI. The invocation is a plain HTTP request to the route's invoke URL, which is
/// why this adapter is excluded from coverage and exercised by the live smoke instead of unit tests.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Performs live HTTP and is verified by smoke testing.")]
internal sealed class HttpApiRouteInvoker : IHttpApiRouteInvoker
{
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

    public async Task<Result<HttpRouteInvocationResult>> InvokeAsync(
        string requestUri,
        string method,
        string? bearerToken,
        string? body,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = new HttpClient { Timeout = _timeout };
            using var message = new HttpRequestMessage(new HttpMethod(method), requestUri);

            if (!string.IsNullOrWhiteSpace(bearerToken))
                message.Headers.TryAddWithoutValidation("Authorization", $"Bearer {bearerToken}");

            if (body is not null)
                message.Content = new StringContent(body);

            var stopwatch = Stopwatch.StartNew();
            using var response = await client.SendAsync(message, cancellationToken);
            stopwatch.Stop();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            var headers = response.Headers
                .Concat(response.Content.Headers)
                .ToDictionary(
                    header => header.Key,
                    header => string.Join(", ", header.Value),
                    StringComparer.OrdinalIgnoreCase);

            var statusCode = (int)response.StatusCode;
            var authorized = statusCode is not (401 or 403);

            return new HttpRouteInvocationResult(
                statusCode,
                authorized,
                stopwatch.ElapsedMilliseconds,
                headers,
                responseBody);
        }
        catch (Exception exception) when (
            exception is HttpRequestException
                or TaskCanceledException
                or OperationCanceledException
                or InvalidOperationException
                or UriFormatException
                or FormatException)
        {
            return new Error($"Unable to invoke route at {requestUri}: {exception.Message}");
        }
    }
}
