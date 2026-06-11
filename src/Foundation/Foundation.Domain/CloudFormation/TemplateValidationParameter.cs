namespace Foundation.Domain.CloudFormation;

/// <summary>
/// A single input parameter that a CloudFormation template declares, as reported by template validation.
/// </summary>
/// <param name="ParameterKey">The name of the parameter the template declares.</param>
/// <param name="DefaultValue">The default value the template assigns to the parameter, or an empty string when none.</param>
/// <param name="NoEcho">Whether the parameter value is masked in the console and logs.</param>
/// <param name="Description">The description the template documents for the parameter, or an empty string when none.</param>
public sealed record TemplateValidationParameter(
    string ParameterKey,
    string DefaultValue,
    bool NoEcho,
    string Description);
