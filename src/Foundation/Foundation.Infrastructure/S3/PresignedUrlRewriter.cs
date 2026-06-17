using Foundation.Application.S3;

namespace Foundation.Infrastructure.S3;

/// <summary>
/// Rewrites the scheme and authority of an S3 presigned URL to a browser-reachable endpoint.
/// The public endpoint is taken from <c>LSW_PUBLIC_AWS_ENDPOINT_URL</c> when configured; otherwise
/// it is derived from the internal endpoint by swapping the host for <c>localhost</c> over HTTP
/// while preserving the port. The signed path and query are left untouched.
/// </summary>
internal sealed class PresignedUrlRewriter : IPresignedUrlRewriter
{
    private readonly Uri? _publicBase;

    public PresignedUrlRewriter(string? internalEndpoint, string? configuredPublicEndpoint)
        => _publicBase = ResolvePublicBase(internalEndpoint, configuredPublicEndpoint);

    public string Rewrite(string url)
    {
        if (_publicBase is null || !Uri.TryCreate(url, UriKind.Absolute, out var original))
            return url;

        var originAuthority = original.GetLeftPart(UriPartial.Authority);
        var publicAuthority = _publicBase.GetLeftPart(UriPartial.Authority);
        if (string.Equals(originAuthority, publicAuthority, StringComparison.Ordinal))
            return url;

        return string.Concat(publicAuthority, url.AsSpan(originAuthority.Length));
    }

    private static Uri? ResolvePublicBase(string? internalEndpoint, string? configuredPublicEndpoint)
    {
        if (!string.IsNullOrWhiteSpace(configuredPublicEndpoint)
            && Uri.TryCreate(configuredPublicEndpoint, UriKind.Absolute, out var configured))
        {
            return configured;
        }

        var port = Uri.TryCreate(internalEndpoint, UriKind.Absolute, out var internalUri) && !internalUri.IsDefaultPort
            ? internalUri.Port
            : 4566;
        return new Uri($"http://localhost:{port}");
    }
}
