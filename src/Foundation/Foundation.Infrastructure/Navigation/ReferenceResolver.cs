using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Navigation;
using Foundation.Domain.Navigation;

namespace Foundation.Infrastructure.Navigation;

/// <summary>
/// Maps an AWS resource reference onto the managing service and a relative SPA route, so the UI can
/// turn a cross-resource reference into a link. Operates purely on the supplied string and a static
/// service map; it performs no backend calls.
/// </summary>
internal sealed class ReferenceResolver : IReferenceResolver
{
    private static readonly Dictionary<string, string> _catalogueKeyByAlias =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["sqs"] = "sqs",
            ["sns"] = "sns",
            ["lambda"] = "lambda",
            ["s3"] = "s3",
            ["dynamodb"] = "dynamodb",
            ["logs"] = "cloudwatch-logs",
            ["cloudwatch-logs"] = "cloudwatch-logs",
            ["secretsmanager"] = "secrets-manager",
            ["secrets-manager"] = "secrets-manager",
            ["ssm"] = "ssm-parameter-store",
            ["ssm-parameter-store"] = "ssm-parameter-store",
            ["states"] = "step-functions",
            ["step-functions"] = "step-functions",
        };

    public Result<ResourceReference> Resolve(string reference, string? service = null)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return new Error("A resource reference is required.");

        string alias;
        string resourceId;

        if (ArnParts.TryParse(reference, out var arn))
        {
            alias = arn.Service;
            resourceId = arn.ResourceId;
        }
        else if (reference.StartsWith("arn:", StringComparison.OrdinalIgnoreCase))
        {
            return new Error($"'{reference}' is not a valid ARN.");
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
}
