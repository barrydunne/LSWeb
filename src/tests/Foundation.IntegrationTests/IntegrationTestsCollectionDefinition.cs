namespace Foundation.IntegrationTests;

/// <summary>
/// Shares a single in-process API host across all integration test classes so
/// that multiple web application factory instances do not race to build the
/// host in parallel.
/// </summary>
[CollectionDefinition(Name)]
public sealed class IntegrationTestsCollectionDefinition : ICollectionFixture<IntegrationTestsFixture>
{
    public const string Name = "Integration tests";
}
