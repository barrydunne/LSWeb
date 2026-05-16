namespace Foundation.Infrastructure.Diagnostics;

/// <summary>
/// Settings that govern whether sensitive diagnostic values may be revealed.
/// </summary>
/// <param name="AllowReveal">Whether the host permits an explicit reveal of sensitive values.</param>
internal sealed record RedactionSettings(bool AllowReveal);
