using Foundation.Application.Diagnostics;
using Foundation.Domain.Configuration;

namespace Foundation.Infrastructure.Diagnostics;

/// <summary>
/// Redacts sensitive configuration values, honouring a reveal request only when the host
/// has been explicitly configured to permit it.
/// </summary>
internal sealed class RedactionService : IRedactionService
{
    private readonly bool _allowReveal;

    public RedactionService(RedactionSettings settings)
        => _allowReveal = settings.AllowReveal;

    public bool CanReveal => _allowReveal;

    public string Resolve(ConfigValue value, bool reveal)
    {
        if (!value.IsSensitive)
            return value.Value;

        return reveal && _allowReveal ? value.Value : value.Display;
    }

    public string ResolveUserSecret(ConfigValue value, bool reveal)
    {
        if (!value.IsSensitive)
            return value.Value;

        return reveal ? value.Value : value.Display;
    }
}
