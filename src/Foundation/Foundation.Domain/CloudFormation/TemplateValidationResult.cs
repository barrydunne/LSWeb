namespace Foundation.Domain.CloudFormation;

/// <summary>
/// The outcome of validating a CloudFormation template, summarising what the backend understood about it.
/// </summary>
/// <param name="Description">The description the template documents, or an empty string when none.</param>
/// <param name="CapabilitiesReason">The reason the template requires the reported capabilities, or an empty string when none.</param>
/// <param name="Capabilities">The capabilities the template requires, such as CAPABILITY_IAM.</param>
/// <param name="Parameters">The input parameters the template declares.</param>
public sealed record TemplateValidationResult(
    string Description,
    string CapabilitiesReason,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<TemplateValidationParameter> Parameters);
