namespace Foundation.Domain.S3;

/// <summary>
/// The way an S3 object preview should be rendered to the user.
/// </summary>
public enum S3PreviewKind
{
    /// <summary>Plain or structured text that can be shown verbatim.</summary>
    Text,

    /// <summary>JSON content that can be pretty-printed in a structured viewer.</summary>
    Json,

    /// <summary>An image that can be rendered inline.</summary>
    Image,

    /// <summary>Opaque binary content with no inline representation.</summary>
    Binary,
}
