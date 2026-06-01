namespace Foundation.Domain.CloudFormation;

/// <summary>
/// A tag applied to a CloudFormation stack.
/// </summary>
/// <param name="Key">The tag key.</param>
/// <param name="Value">The tag value.</param>
public sealed record StackTag(
    string Key,
    string Value);
