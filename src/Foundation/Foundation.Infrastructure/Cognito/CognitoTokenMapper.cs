using System.Text;
using System.Text.Json;
using Foundation.Domain.Cognito;

namespace Foundation.Infrastructure.Cognito;

/// <summary>
/// Decodes the claims contained within a Cognito-issued JSON Web Token so they can be inspected
/// without verifying the signature, which is sufficient for local token-flow testing.
/// </summary>
internal static class CognitoTokenMapper
{
    /// <summary>
    /// Decodes the claims from the payload segment of a JWT. Returns an empty list when the token is
    /// absent, malformed, or its payload is not valid JSON.
    /// </summary>
    /// <param name="jwt">The JSON Web Token to decode.</param>
    /// <returns>The claims as name/value pairs.</returns>
    public static List<CognitoUserAttributeEntry> DecodeClaims(string? jwt)
    {
        var claims = new List<CognitoUserAttributeEntry>();
        if (string.IsNullOrEmpty(jwt))
            return claims;

        var parts = jwt.Split('.');
        if (parts.Length < 2)
            return claims;

        var payload = DecodeBase64Url(parts[1]);
        if (payload is null)
            return claims;

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return claims;

            foreach (var property in document.RootElement.EnumerateObject())
                claims.Add(new CognitoUserAttributeEntry(property.Name, property.Value.ToString()));
        }
        catch (JsonException)
        {
            return [];
        }

        return claims;
    }

    private static string? DecodeBase64Url(string value)
    {
        var normalized = value.Replace('-', '+').Replace('_', '/');
        normalized = (normalized.Length % 4) switch
        {
            2 => normalized + "==",
            3 => normalized + "=",
            _ => normalized,
        };

        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
