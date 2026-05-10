using Microsoft.AspNetCore.Mvc.Testing;

namespace Foundation.IntegrationTests;

/// <summary>
/// Hosts the API in-process for integration tests. Future slices extend this
/// harness with Testcontainers-backed dependencies (for example LocalStack).
/// </summary>
public sealed class IntegrationTestsFixture : WebApplicationFactory<Program>
{
}
