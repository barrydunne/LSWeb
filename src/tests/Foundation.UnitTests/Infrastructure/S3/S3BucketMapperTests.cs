using System.Globalization;
using Foundation.Infrastructure.S3;

namespace Foundation.UnitTests.Infrastructure.S3;

public class S3BucketMapperTests
{
    [Fact]
    public void ToBucket_WithPopulatedFields_MapsAllValues()
    {
        // Arrange
        var creationDate = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var bucket = new Amazon.S3.Model.S3Bucket
        {
            BucketName = "orders",
            CreationDate = creationDate,
        };

        // Act
        var result = S3BucketMapper.ToBucket(bucket);

        // Assert
        result.Name.Should().Be("orders");
        result.CreationDate.Should().Be(creationDate.ToString("O", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ToBucket_WithUnsetFields_AppliesSafeDefaults()
    {
        // Arrange
        var bucket = new Amazon.S3.Model.S3Bucket();

        // Act
        var result = S3BucketMapper.ToBucket(bucket);

        // Assert
        result.Name.Should().BeEmpty();
        result.CreationDate.Should().BeEmpty();
    }
}
