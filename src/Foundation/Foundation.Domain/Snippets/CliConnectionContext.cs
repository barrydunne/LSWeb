namespace Foundation.Domain.Snippets;

/// <summary>
/// The connection context used to target a generated CLI snippet at a specific endpoint.
/// </summary>
/// <param name="Endpoint">The endpoint URL passed via <c>--endpoint-url</c>.</param>
/// <param name="Region">The AWS region passed via <c>--region</c>.</param>
/// <param name="Profile">An optional named profile passed via <c>--profile</c>; omitted when absent.</param>
public sealed record CliConnectionContext(string Endpoint, string Region, string? Profile = null);
