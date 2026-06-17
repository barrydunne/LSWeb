using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Navigation;
using Foundation.Domain.Catalogue;
using Foundation.Domain.Navigation;

namespace Foundation.Infrastructure.Navigation;

/// <summary>
/// Maps an AWS resource reference onto the managing service and a relative SPA route, so the UI can
/// turn a cross-resource reference into a link. Operates purely on the supplied string and a static
/// service map; it performs no backend calls.
/// </summary>
internal sealed class ReferenceResolver : IReferenceResolver
{
    /// <summary>
    /// Extra aliases for AWS ARN/SDK service names that differ from the catalogue key (for example
    /// the ARN service <c>logs</c> maps to the <c>cloudwatch-logs</c> catalogue entry).
    /// </summary>
    private static readonly Dictionary<string, string> _extraAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["logs"] = "cloudwatch-logs",
            ["cognito-idp"] = "cognito",
            ["secretsmanager"] = "secrets-manager",
            ["ssm"] = "ssm-parameter-store",
            ["states"] = "step-functions",
        };

    /// <summary>
    /// Every catalogue service key mapped to itself, plus the ARN/SDK aliases. Derived from the
    /// <see cref="ServiceCatalogue"/> so every service that can be recorded as a recent destination
    /// resolves, and new services are supported automatically without updating this map.
    /// </summary>
    private static readonly Dictionary<string, string> _catalogueKeyByAlias = BuildAliasMap();

    private static Dictionary<string, string> BuildAliasMap()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var service in ServiceCatalogue.Services)
            map[service.Key] = service.Key;
        foreach (var alias in _extraAliases)
            map[alias.Key] = alias.Value;
        return map;
    }

    /// <summary>
    /// Services whose resource ids carry a type prefix (for example <c>role/Name</c> or
    /// <c>user/Name</c>). The prefix is preserved in the route so the detail view can dispatch by
    /// resource type rather than receiving a bare name.
    /// </summary>
    private static readonly HashSet<string> _typePrefixedAliases =
        new(StringComparer.OrdinalIgnoreCase) { "iam" };

    /// <summary>
    /// Services whose detail view is keyed by the full ARN rather than the bare resource name (for
    /// example the SNS topic detail loads subscriptions by topic ARN). For these, an ARN reference
    /// keeps the whole ARN as the route id so the link matches how the service list links the
    /// resource.
    /// </summary>
    private static readonly HashSet<string> _arnKeyedAliases =
        new(StringComparer.OrdinalIgnoreCase) { "sns" };

    public Result<ResourceReference> Resolve(string reference, string? service = null)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return new Error("A resource reference is required.");

        string alias;
        string resourceId;

        if (ArnParts.TryParse(reference, out var arn))
        {
            alias = arn.Service;
            resourceId = _arnKeyedAliases.Contains(alias)
                ? reference
                : _typePrefixedAliases.Contains(alias) ? arn.Resource : arn.ResourceId;
        }
        else if (reference.StartsWith("arn:", StringComparison.OrdinalIgnoreCase))
        {
            return new Error($"'{reference}' is not a valid ARN.");
        }
        else if (TryParseSchemeReference(reference, out var schemeAlias, out var schemeResourceId))
        {
            alias = schemeAlias;
            resourceId = schemeResourceId;
        }
        else if (string.IsNullOrWhiteSpace(service))
        {
            return new Error("A service is required to resolve a non-ARN reference.");
        }
        else
        {
            alias = service;
            resourceId = reference;
        }

        if (!_catalogueKeyByAlias.TryGetValue(alias, out var serviceKey))
            return new Error($"Unsupported service '{alias}'.");

        var route = $"/services/{serviceKey}/{Uri.EscapeDataString(resourceId)}";
        return new ResourceReference(serviceKey, resourceId, route);
    }

    /// <summary>
    /// Parses a <c>scheme://resourceId</c> reference (for example <c>sqs://orders</c>), the form the
    /// UI records for recently-viewed resources. Splits on the first <c>://</c> so resource ids that
    /// themselves contain slashes survive intact.
    /// </summary>
    private static bool TryParseSchemeReference(string reference, out string alias, out string resourceId)
    {
        var separatorIndex = reference.IndexOf("://", StringComparison.Ordinal);
        if (separatorIndex > 0)
        {
            var candidateResourceId = reference[(separatorIndex + 3)..];
            if (!string.IsNullOrWhiteSpace(candidateResourceId))
            {
                alias = reference[..separatorIndex];
                resourceId = candidateResourceId;
                return true;
            }
        }

        alias = string.Empty;
        resourceId = string.Empty;
        return false;
    }
}
