using System.Text;
using Foundation.Domain.Cognito;
using Foundation.Infrastructure.Cognito;

namespace Foundation.UnitTests.Infrastructure.Cognito;

public class CognitoTokenMapperTests
{
    private static string ToJwt(string json)
    {
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return $"header.{payload}.signature";
    }

    [Fact]
    public void DecodeClaims_WhenTokenHasClaims_ReturnsNameValuePairs()
    {
        // Arrange
        var jwt = ToJwt("{\"sub\":\"abc\",\"email\":\"alice@example.com\",\"exp\":1700000000}");

        // Act
        var claims = CognitoTokenMapper.DecodeClaims(jwt);

        // Assert
        claims.Should().Contain(new CognitoUserAttributeEntry("sub", "abc"));
        claims.Should().Contain(new CognitoUserAttributeEntry("email", "alice@example.com"));
        claims.Should().Contain(new CognitoUserAttributeEntry("exp", "1700000000"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void DecodeClaims_WhenTokenMissing_ReturnsEmpty(string? jwt)
    {
        var claims = CognitoTokenMapper.DecodeClaims(jwt);
        claims.Should().BeEmpty();
    }

    [Fact]
    public void DecodeClaims_WhenTokenHasNoPayloadSegment_ReturnsEmpty()
    {
        var claims = CognitoTokenMapper.DecodeClaims("single-segment");
        claims.Should().BeEmpty();
    }

    [Fact]
    public void DecodeClaims_WhenPayloadIsNotValidBase64_ReturnsEmpty()
    {
        var claims = CognitoTokenMapper.DecodeClaims("header.!!!.signature");
        claims.Should().BeEmpty();
    }

    [Fact]
    public void DecodeClaims_WhenPayloadIsNotJson_ReturnsEmpty()
    {
        var jwt = ToJwt("not-json");
        var claims = CognitoTokenMapper.DecodeClaims(jwt);
        claims.Should().BeEmpty();
    }

    [Fact]
    public void DecodeClaims_WhenPayloadIsNotAnObject_ReturnsEmpty()
    {
        var jwt = ToJwt("[1,2,3]");
        var claims = CognitoTokenMapper.DecodeClaims(jwt);
        claims.Should().BeEmpty();
    }

    [Fact]
    public void DecodeClaims_WhenPayloadIsEmptyObject_ReturnsEmpty()
    {
        var jwt = ToJwt("{}");
        var claims = CognitoTokenMapper.DecodeClaims(jwt);
        claims.Should().BeEmpty();
    }

    [Fact]
    public void DecodeClaims_WhenPayloadHasNoBase64Padding_ReturnsClaims()
    {
        // A 15-byte payload (a multiple of three) produces base64 with no padding.
        var jwt = ToJwt("{\"a\":\"b\",\"c\":1}");

        var claims = CognitoTokenMapper.DecodeClaims(jwt);

        claims.Should().Contain(new CognitoUserAttributeEntry("a", "b"));
        claims.Should().Contain(new CognitoUserAttributeEntry("c", "1"));
    }
}
