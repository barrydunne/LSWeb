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

    [Fact]
    public void ToObject_WithPopulatedFields_MapsAllValues()
    {
        // Arrange
        var lastModified = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var s3Object = new Amazon.S3.Model.S3Object
        {
            Key = "orders/readme.txt",
            Size = 42,
            LastModified = lastModified,
        };

        // Act
        var result = S3BucketMapper.ToObject(s3Object);

        // Assert
        result.Key.Should().Be("orders/readme.txt");
        result.Size.Should().Be(42);
        result.LastModified.Should().Be(lastModified.ToString("O", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ToObject_WithUnsetFields_AppliesSafeDefaults()
    {
        // Arrange
        var s3Object = new Amazon.S3.Model.S3Object();

        // Act
        var result = S3BucketMapper.ToObject(s3Object);

        // Assert
        result.Key.Should().BeEmpty();
        result.Size.Should().Be(0);
        result.LastModified.Should().BeEmpty();
    }
}
