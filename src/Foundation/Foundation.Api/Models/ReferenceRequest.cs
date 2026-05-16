namespace Foundation.Api.Models;

/// <summary>
/// A request carrying a single resource reference to record or pin.
/// </summary>
/// <param name="Reference">The resource reference (ARN or identifier).</param>
public sealed record ReferenceRequest(string Reference);
