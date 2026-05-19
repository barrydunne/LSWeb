using Foundation.Application.Lambda;

namespace Foundation.UnitTests.Application.Lambda;

public class Base64PayloadTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not valid base64!!!")]
    public void IsValid_WhenNullEmptyOrMalformed_ReturnsFalse(string? value)
        => Base64Payload.IsValid(value).Should().BeFalse();

    [Fact]
    public void IsValid_WhenWellFormedBase64_ReturnsTrue()
        => Base64Payload.IsValid("QkFTRTY0").Should().BeTrue();
}
