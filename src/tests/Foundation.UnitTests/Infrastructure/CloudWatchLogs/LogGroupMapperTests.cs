using Foundation.Infrastructure.CloudWatchLogs;
using SdkLogGroup = Amazon.CloudWatchLogs.Model.LogGroup;

namespace Foundation.UnitTests.Infrastructure.CloudWatchLogs;

public class LogGroupMapperTests
{
    [Fact]
    public void ToLogGroup_MapsAllProperties()
    {
        // Arrange
        var creation = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Unspecified);
        var sdk = new SdkLogGroup
        {
            LogGroupName = "/aws/lambda/orders",
            Arn = "arn:aws:logs:eu-west-1:000000000000:log-group:/aws/lambda/orders",
            StoredBytes = 1024,
            RetentionInDays = 7,
            CreationTime = creation,
        };

        // Act
        var group = LogGroupMapper.ToLogGroup(sdk);

        // Assert
        group.Name.Should().Be("/aws/lambda/orders");
        group.Arn.Should().Be("arn:aws:logs:eu-west-1:000000000000:log-group:/aws/lambda/orders");
        group.StoredBytes.Should().Be(1024);
        group.RetentionInDays.Should().Be(7);
        group.CreatedAt.Should().Be(new DateTimeOffset(creation, TimeSpan.Zero));
    }

    [Fact]
    public void ToLogGroup_WhenValuesNull_AppliesDefaults()
    {
        // Arrange
        var sdk = new SdkLogGroup();

        // Act
        var group = LogGroupMapper.ToLogGroup(sdk);

        // Assert
        group.Name.Should().BeEmpty();
        group.Arn.Should().BeEmpty();
        group.StoredBytes.Should().Be(0);
        group.RetentionInDays.Should().BeNull();
        group.CreatedAt.Should().BeNull();
    }
}
