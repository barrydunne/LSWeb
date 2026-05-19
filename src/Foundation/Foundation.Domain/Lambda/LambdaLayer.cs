namespace Foundation.Domain.Lambda;

/// <summary>
/// A Lambda layer version attached to a function, shown on the function detail view.
/// </summary>
/// <param name="Arn">The full Amazon Resource Name of the attached layer version; empty when not reported.</param>
/// <param name="Name">The layer name derived from the ARN; empty when it cannot be derived.</param>
/// <param name="Version">The layer version derived from the ARN; empty when it cannot be derived.</param>
public sealed record LambdaLayer(
    string Arn,
    string Name,
    string Version);
