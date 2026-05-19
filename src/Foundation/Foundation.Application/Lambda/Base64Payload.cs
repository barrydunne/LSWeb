namespace Foundation.Application.Lambda;

/// <summary>
/// Helpers for validating base64-encoded Lambda deployment packages before they reach the AWS gateway.
/// </summary>
internal static class Base64Payload
{
    /// <summary>
    /// Determine whether the supplied value is a non-empty, well-formed base64 string.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns><c>true</c> when the value can be decoded as base64; otherwise <c>false</c>.</returns>
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var buffer = new byte[(value.Length + 3) / 4 * 3];
        return Convert.TryFromBase64String(value, buffer, out _);
    }
}
