namespace Foundation.Application.S3;

/// <summary>
/// Rewrites an S3 presigned URL so its host is reachable from the user's browser. The AWS client is
/// configured with the internal backend endpoint (for example <c>http://localstack:4566</c> on the
/// container network), which a user's browser cannot resolve; this swaps the scheme and authority
/// for the public, user-facing endpoint while preserving the signed path and query verbatim.
/// </summary>
public interface IPresignedUrlRewriter
{
    /// <summary>
    /// Rewrite the scheme and authority of a presigned URL to the public-facing endpoint.
    /// </summary>
    /// <param name="url">The presigned URL produced against the internal endpoint.</param>
    /// <returns>The URL with a browser-reachable host, or the original string when no rewrite applies.</returns>
    string Rewrite(string url);
}
