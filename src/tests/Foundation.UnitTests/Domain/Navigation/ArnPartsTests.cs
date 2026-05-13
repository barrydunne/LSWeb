using Foundation.Domain.Navigation;

namespace Foundation.UnitTests.Domain.Navigation;

public class ArnPartsTests
{
    [Fact]
    public void TryParse_WhenWellFormed_ReturnsParts()
    {
        var parsed = ArnParts.TryParse("arn:aws:sqs:eu-west-1:000000000000:my-queue", out var parts);

        parsed.Should().BeTrue();
        parts.Should().Be(new ArnParts("aws", "sqs", "eu-west-1", "000000000000", "my-queue"));
        parts!.Partition.Should().Be("aws");
        parts.Region.Should().Be("eu-west-1");
        parts.AccountId.Should().Be("000000000000");
    }

    [Fact]
    public void TryParse_WhenGlobalServiceOmitsRegionAndAccount_ReturnsParts()
    {
        var parsed = ArnParts.TryParse("arn:aws:s3:::my-bucket", out var parts);

        parsed.Should().BeTrue();
        parts.Should().Be(new ArnParts("aws", "s3", string.Empty, string.Empty, "my-bucket"));
    }

    [Fact]
    public void TryParse_WhenArnLiteralUppercase_ReturnsParts()
    {
        var parsed = ArnParts.TryParse("ARN:aws:lambda:eu-west-1:000000000000:function:my-func", out var parts);

        parsed.Should().BeTrue();
        parts!.Resource.Should().Be("function:my-func");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParse_WhenNullOrWhitespace_ReturnsFalse(string? value)
    {
        var parsed = ArnParts.TryParse(value, out var parts);

        parsed.Should().BeFalse();
        parts.Should().BeNull();
    }

    [Fact]
    public void TryParse_WhenTooFewSegments_ReturnsFalse()
    {
        var parsed = ArnParts.TryParse("arn:aws:sqs", out var parts);

        parsed.Should().BeFalse();
        parts.Should().BeNull();
    }

    [Fact]
    public void TryParse_WhenFirstSegmentIsNotArn_ReturnsFalse()
    {
        var parsed = ArnParts.TryParse("xrn:aws:sqs:eu-west-1:000000000000:my-queue", out var parts);

        parsed.Should().BeFalse();
        parts.Should().BeNull();
    }

    [Fact]
    public void TryParse_WhenPartitionEmpty_ReturnsFalse()
    {
        var parsed = ArnParts.TryParse("arn::sqs:eu-west-1:000000000000:my-queue", out var parts);

        parsed.Should().BeFalse();
        parts.Should().BeNull();
    }

    [Fact]
    public void TryParse_WhenServiceEmpty_ReturnsFalse()
    {
        var parsed = ArnParts.TryParse("arn:aws::eu-west-1:000000000000:my-queue", out var parts);

        parsed.Should().BeFalse();
        parts.Should().BeNull();
    }

    [Fact]
    public void TryParse_WhenResourceEmpty_ReturnsFalse()
    {
        var parsed = ArnParts.TryParse("arn:aws:sqs:eu-west-1:000000000000:", out var parts);

        parsed.Should().BeFalse();
        parts.Should().BeNull();
    }

    [Fact]
    public void ResourceId_WhenResourceHasColonPrefix_ReturnsBareIdentifier()
    {
        ArnParts.TryParse("arn:aws:lambda:eu-west-1:000000000000:function:my-func", out var parts).Should().BeTrue();

        parts!.ResourceId.Should().Be("my-func");
    }

    [Fact]
    public void ResourceId_WhenResourceHasSlashPrefix_ReturnsBareIdentifier()
    {
        ArnParts.TryParse("arn:aws:dynamodb:eu-west-1:000000000000:table/my-table", out var parts).Should().BeTrue();

        parts!.ResourceId.Should().Be("my-table");
    }

    [Fact]
    public void ResourceId_WhenResourceHasNoPrefix_ReturnsResourceUnchanged()
    {
        ArnParts.TryParse("arn:aws:sqs:eu-west-1:000000000000:my-queue", out var parts).Should().BeTrue();

        parts!.ResourceId.Should().Be("my-queue");
    }
}
