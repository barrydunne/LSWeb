using System.Diagnostics.CodeAnalysis;

namespace Foundation.Api;

/// <summary>
/// Performs a one-shot HTTP liveness probe against a running instance of the
/// application. Used as the container <c>HEALTHCHECK</c> entry point.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "CLI health probe exercised via the container HEALTHCHECK; not unit tested.")]
internal static class HealthCheckProbe
{
    /// <summary>
    /// Issues a GET request to the local liveness endpoint.
    /// </summary>
    /// <param name="port">The port the application is listening on.</param>
    /// <returns><c>0</c> when the endpoint reports success; otherwise <c>1</c>.</returns>
    public static async Task<int> RunAsync(string port)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            using var response = await client.GetAsync(new Uri($"http://localhost:{port}/api/system/liveness"));
            return response.IsSuccessStatusCode ? 0 : 1;
        }
        catch (Exception)
        {
            return 1;
        }
    }
}
