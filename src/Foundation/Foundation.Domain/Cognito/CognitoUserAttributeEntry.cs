namespace Foundation.Domain.Cognito;

/// <summary>
/// A single attribute of an Amazon Cognito user, such as an email address or a custom value.
/// </summary>
/// <param name="Name">The name of the attribute (for example <c>email</c> or <c>phone_number</c>).</param>
/// <param name="Value">The value of the attribute.</param>
public sealed record CognitoUserAttributeEntry(
    string Name,
    string Value);
