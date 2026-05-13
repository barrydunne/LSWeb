namespace Foundation.Domain.Navigation;

/// <summary>
/// A navigable pointer to a managed resource, produced when one resource references another (for
/// example a Lambda event source or an SNS subscription). Carries everything the UI needs to render
/// a link that opens the target resource.
/// </summary>
/// <param name="ServiceKey">The catalogue key of the owning service, for example <c>sqs</c>.</param>
/// <param name="ResourceId">The bare identifier of the target resource, for example a queue name.</param>
/// <param name="Route">The relative SPA route that opens the target resource.</param>
public sealed record ResourceReference(
    string ServiceKey,
    string ResourceId,
    string Route);
