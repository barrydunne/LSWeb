namespace Foundation.Domain.Iam;

/// <summary>
/// A key/value tag attached to an IAM user, role, or managed policy.
/// </summary>
/// <param name="Key">The tag key.</param>
/// <param name="Value">The tag value.</param>
public sealed record IamTag(string Key, string Value);
