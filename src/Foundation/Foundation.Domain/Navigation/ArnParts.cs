using System.Diagnostics.CodeAnalysis;

namespace Foundation.Domain.Navigation;

/// <summary>
/// The structured segments of an Amazon Resource Name (ARN), broken out so a reference can be
/// mapped to the service and resource it points at without re-parsing the original string.
/// </summary>
/// <param name="Partition">The ARN partition, for example <c>aws</c>.</param>
/// <param name="Service">The service namespace, for example <c>sqs</c> or <c>states</c>.</param>
/// <param name="Region">The region the resource lives in; empty for global services such as S3.</param>
/// <param name="AccountId">The owning account identifier; empty for services that omit it.</param>
/// <param name="Resource">The resource portion, which may include a type prefix such as <c>function:name</c>.</param>
public sealed record ArnParts(
    string Partition,
    string Service,
    string Region,
    string AccountId,
    string Resource)
{
    /// <summary>
    /// Gets the bare resource identifier with any resource-type prefix removed, for example
    /// <c>my-name</c> from <c>function:my-name</c> or <c>table/my-name</c>.
    /// </summary>
    public string ResourceId
    {
        get
        {
            var separator = Resource.LastIndexOfAny([':', '/']);
            return separator >= 0 ? Resource[(separator + 1)..] : Resource;
        }
    }

    /// <summary>
    /// Attempts to parse a string of the form <c>arn:partition:service:region:account-id:resource</c>
    /// into its parts.
    /// </summary>
    /// <param name="value">The candidate ARN string.</param>
    /// <param name="parts">The parsed parts when the value is a well-formed ARN; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when the value is a well-formed ARN; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(string? value, [NotNullWhen(true)] out ArnParts? parts)
    {
        parts = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var segments = value.Split(':', 6);
        if (segments.Length != 6 || !string.Equals(segments[0], "arn", StringComparison.OrdinalIgnoreCase))
            return false;

        if (segments[1].Length == 0 || segments[2].Length == 0 || segments[5].Length == 0)
            return false;

        parts = new ArnParts(segments[1], segments[2], segments[3], segments[4], segments[5]);
        return true;
    }
}
