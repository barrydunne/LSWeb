namespace Foundation.Domain.Cognito;

/// <summary>
/// The desired configuration of an Amazon Cognito user, used when creating one.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool the user belongs to.</param>
/// <param name="Username">The unique username of the user within the pool.</param>
/// <param name="Attributes">The attributes to record against the user.</param>
/// <param name="TemporaryPassword">An optional temporary password to assign to the new user.</param>
public sealed record CognitoUserSpecification(
    string UserPoolId,
    string Username,
    IReadOnlyList<CognitoUserAttributeEntry> Attributes,
    string? TemporaryPassword);
