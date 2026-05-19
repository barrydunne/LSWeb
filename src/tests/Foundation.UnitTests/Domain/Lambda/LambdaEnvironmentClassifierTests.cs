using Foundation.Domain.Lambda;

namespace Foundation.UnitTests.Domain.Lambda;

public class LambdaEnvironmentClassifierTests
{
    [Theory]
    [InlineData("API_SECRET")]
    [InlineData("DB_PASSWORD")]
    [InlineData("ftp_passwd")]
    [InlineData("ACCESS_TOKEN")]
    [InlineData("API_KEY")]
    [InlineData("DB_CREDENTIAL")]
    [InlineData("PRIVATE_VALUE")]
    public void IsSensitive_WhenNameContainsMarker_ReturnsTrue(string name)
        => LambdaEnvironmentClassifier.IsSensitive(name).Should().BeTrue();

    [Theory]
    [InlineData("REGION")]
    [InlineData("STAGE")]
    [InlineData("LOG_LEVEL")]
    public void IsSensitive_WhenNameHasNoMarker_ReturnsFalse(string name)
        => LambdaEnvironmentClassifier.IsSensitive(name).Should().BeFalse();
}
