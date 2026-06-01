namespace Foundation.Domain.CloudFormation;

/// <summary>
/// The template that defines a CloudFormation stack.
/// </summary>
/// <param name="TemplateBody">The raw template body as stored by the backend.</param>
/// <param name="Format">The detected template format, either <c>json</c> or <c>yaml</c>.</param>
public sealed record CloudFormationStackTemplate(
    string TemplateBody,
    string Format);
