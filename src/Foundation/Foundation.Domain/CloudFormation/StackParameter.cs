namespace Foundation.Domain.CloudFormation;

/// <summary>
/// An input parameter a CloudFormation stack was deployed with.
/// </summary>
/// <param name="ParameterKey">The parameter name.</param>
/// <param name="ParameterValue">The parameter value.</param>
public sealed record StackParameter(
    string ParameterKey,
    string ParameterValue);
