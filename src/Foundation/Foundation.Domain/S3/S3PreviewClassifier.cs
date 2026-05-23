namespace Foundation.Domain.S3;

/// <summary>
/// Classifies how an S3 object should be previewed using its reported content type and key,
/// so the user interface can choose between a text, JSON, image, or binary rendering.
/// </summary>
public static class S3PreviewClassifier
{
    private static readonly HashSet<string> _imageExtensions = new(StringComparer.Ordinal)
    {
        "png", "jpg", "jpeg", "gif", "bmp", "webp", "svg", "ico",
    };

    private static readonly HashSet<string> _textTypes = new(StringComparer.Ordinal)
    {
        "application/xml", "application/javascript", "application/x-yaml", "application/yaml", "application/x-sh",
    };

    private static readonly HashSet<string> _textExtensions = new(StringComparer.Ordinal)
    {
        "txt", "md", "csv", "log", "xml", "yaml", "yml", "html", "htm", "js", "ts", "css", "ini", "conf", "sh", "sql",
    };

    /// <summary>
    /// Determines the preview kind for an object.
    /// </summary>
    /// <param name="contentType">The reported content type of the object.</param>
    /// <param name="key">The object key, used as an extension-based fallback.</param>
    /// <returns>The most appropriate <see cref="S3PreviewKind"/>.</returns>
    public static S3PreviewKind Classify(string contentType, string key)
    {
        var type = contentType.Trim().ToLowerInvariant();
        var extension = ExtensionOf(key);

        if (type.StartsWith("image/", StringComparison.Ordinal) || _imageExtensions.Contains(extension))
            return S3PreviewKind.Image;

        if (type.Contains("json", StringComparison.Ordinal) || extension == "json")
            return S3PreviewKind.Json;

        if (type.StartsWith("text/", StringComparison.Ordinal)
            || _textTypes.Contains(type)
            || _textExtensions.Contains(extension))
            return S3PreviewKind.Text;

        return S3PreviewKind.Binary;
    }

    private static string ExtensionOf(string key)
    {
        var dot = key.LastIndexOf('.');
        return dot < 0 ? string.Empty : key[(dot + 1)..].ToLowerInvariant();
    }
}
