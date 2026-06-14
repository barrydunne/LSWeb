using Amazon.CloudWatchLogs.Model;
using Amazon.Lambda.Model;
using Foundation.Infrastructure.Lambda;

namespace Foundation.UnitTests.Infrastructure.Lambda;

public class LambdaFunctionMapperTests
{
    [Fact]
    public void ToSummary_WhenAllFieldsPopulated_MapsEveryValue()
    {
        // Arrange
        var configuration = new FunctionConfiguration
        {
            FunctionName = "process-orders",
            Runtime = Amazon.Lambda.Runtime.Dotnet8,
            Description = "Order processor",
            LastModified = "2026-01-02T03:04:05Z",
            MemorySize = 256,
            Timeout = 30,
        };

        // Act
        var summary = LambdaFunctionMapper.ToSummary(configuration);

        // Assert
        summary.FunctionName.Should().Be("process-orders");
        summary.Runtime.Should().Be("dotnet8");
        summary.Description.Should().Be("Order processor");
        summary.LastModified.Should().Be("2026-01-02T03:04:05Z");
        summary.MemorySize.Should().Be(256);
        summary.Timeout.Should().Be(30);
    }

    [Fact]
    public void ToSummary_WhenFieldsUnset_AppliesSafeDefaults()
    {
        // Arrange
        var configuration = new FunctionConfiguration();

        // Act
        var summary = LambdaFunctionMapper.ToSummary(configuration);

        // Assert
        summary.FunctionName.Should().BeEmpty();
        summary.Runtime.Should().BeEmpty();
        summary.Description.Should().BeEmpty();
        summary.LastModified.Should().BeEmpty();
        summary.MemorySize.Should().Be(0);
        summary.Timeout.Should().Be(0);
    }

    [Fact]
    public void ToDetail_WhenAllFieldsPopulated_MapsEveryValue()
    {
        // Arrange
        var configuration = new FunctionConfiguration
        {
            FunctionName = "process-orders",
            FunctionArn = "arn:aws:lambda:eu-west-1:000000000000:function:process-orders",
            Runtime = Amazon.Lambda.Runtime.Dotnet8,
            Handler = "Orders::Handler",
            Description = "Order processor",
            LastModified = "2026-01-02T03:04:05Z",
            MemorySize = 256,
            Timeout = 30,
            Role = "arn:aws:iam::000000000000:role/lambda-orders",
        };

        // Act
        var detail = LambdaFunctionMapper.ToDetail(configuration);

        // Assert
        detail.FunctionName.Should().Be("process-orders");
        detail.FunctionArn.Should().Be("arn:aws:lambda:eu-west-1:000000000000:function:process-orders");
        detail.Runtime.Should().Be("dotnet8");
        detail.Handler.Should().Be("Orders::Handler");
        detail.Description.Should().Be("Order processor");
        detail.LastModified.Should().Be("2026-01-02T03:04:05Z");
        detail.MemorySize.Should().Be(256);
        detail.Timeout.Should().Be(30);
        detail.Role.Should().Be("arn:aws:iam::000000000000:role/lambda-orders");
    }

    [Fact]
    public void ToDetail_WhenFieldsUnset_AppliesSafeDefaults()
    {
        // Arrange
        var configuration = new FunctionConfiguration();

        // Act
        var detail = LambdaFunctionMapper.ToDetail(configuration);

        // Assert
        detail.FunctionName.Should().BeEmpty();
        detail.FunctionArn.Should().BeEmpty();
        detail.Runtime.Should().BeEmpty();
        detail.Handler.Should().BeEmpty();
        detail.Description.Should().BeEmpty();
        detail.LastModified.Should().BeEmpty();
        detail.MemorySize.Should().Be(0);
        detail.Timeout.Should().Be(0);
        detail.Role.Should().BeEmpty();
    }

    [Fact]
    public void ToCode_WhenZipPackage_MapsConfigurationAndDownloadLocation()
    {
        // Arrange
        var configuration = new FunctionConfiguration
        {
            FunctionName = "process-orders",
            Runtime = Amazon.Lambda.Runtime.Dotnet8,
            Handler = "Orders::Handler",
            PackageType = Amazon.Lambda.PackageType.Zip,
            CodeSize = 2048,
            CodeSha256 = "abc123=",
        };
        var code = new FunctionCodeLocation
        {
            RepositoryType = "S3",
            Location = "https://localstack/download.zip",
        };

        // Act
        var mapped = LambdaFunctionMapper.ToCode(configuration, code);

        // Assert
        mapped.FunctionName.Should().Be("process-orders");
        mapped.Runtime.Should().Be("dotnet8");
        mapped.Handler.Should().Be("Orders::Handler");
        mapped.PackageType.Should().Be("Zip");
        mapped.CodeSize.Should().Be(2048);
        mapped.CodeSha256.Should().Be("abc123=");
        mapped.RepositoryType.Should().Be("S3");
        mapped.Location.Should().Be("https://localstack/download.zip");
        mapped.ImageUri.Should().BeEmpty();
    }

    [Fact]
    public void ToCode_WhenImagePackageWithResolvedUri_UsesResolvedImageUriAsLocation()
    {
        // Arrange
        var configuration = new FunctionConfiguration
        {
            FunctionName = "image-fn",
            PackageType = Amazon.Lambda.PackageType.Image,
        };
        var code = new FunctionCodeLocation
        {
            RepositoryType = "ECR",
            ImageUri = "000000000000.dkr.ecr.eu-west-1.amazonaws.com/app:latest",
            ResolvedImageUri = "000000000000.dkr.ecr.eu-west-1.amazonaws.com/app@sha256:deadbeef",
        };

        // Act
        var mapped = LambdaFunctionMapper.ToCode(configuration, code);

        // Assert
        mapped.PackageType.Should().Be("Image");
        mapped.RepositoryType.Should().Be("ECR");
        mapped.ImageUri.Should().Be("000000000000.dkr.ecr.eu-west-1.amazonaws.com/app:latest");
        mapped.Location.Should().Be("000000000000.dkr.ecr.eu-west-1.amazonaws.com/app@sha256:deadbeef");
    }

    [Fact]
    public void ToCode_WhenImagePackageWithoutResolvedUri_UsesImageUriAsLocation()
    {
        // Arrange
        var configuration = new FunctionConfiguration { FunctionName = "image-fn" };
        var code = new FunctionCodeLocation
        {
            ImageUri = "000000000000.dkr.ecr.eu-west-1.amazonaws.com/app:latest",
        };

        // Act
        var mapped = LambdaFunctionMapper.ToCode(configuration, code);

        // Assert
        mapped.ImageUri.Should().Be("000000000000.dkr.ecr.eu-west-1.amazonaws.com/app:latest");
        mapped.Location.Should().Be("000000000000.dkr.ecr.eu-west-1.amazonaws.com/app:latest");
    }

    [Fact]
    public void ToCode_WhenCodeNullAndFieldsUnset_AppliesSafeDefaults()
    {
        // Arrange
        var configuration = new FunctionConfiguration();

        // Act
        var mapped = LambdaFunctionMapper.ToCode(configuration, null);

        // Assert
        mapped.FunctionName.Should().BeEmpty();
        mapped.Runtime.Should().BeEmpty();
        mapped.Handler.Should().BeEmpty();
        mapped.PackageType.Should().BeEmpty();
        mapped.CodeSize.Should().Be(0);
        mapped.CodeSha256.Should().BeEmpty();
        mapped.RepositoryType.Should().BeEmpty();
        mapped.Location.Should().BeEmpty();
        mapped.ImageUri.Should().BeEmpty();
    }

    [Fact]
    public void ToFunctionUrl_WhenAllFieldsPopulated_MapsEveryValue()
    {
        // Act
        var url = LambdaFunctionMapper.ToFunctionUrl(
            "https://abc.lambda-url.eu-west-1.on.aws/", "NONE", "t1", "t2");

        // Assert
        url.FunctionUrl.Should().Be("https://abc.lambda-url.eu-west-1.on.aws/");
        url.AuthType.Should().Be("NONE");
        url.CreationTime.Should().Be("t1");
        url.LastModifiedTime.Should().Be("t2");
    }

    [Fact]
    public void ToFunctionUrl_WhenFieldsNull_AppliesSafeDefaults()
    {
        // Act
        var url = LambdaFunctionMapper.ToFunctionUrl(null, null, null, null);

        // Assert
        url.FunctionUrl.Should().BeEmpty();
        url.AuthType.Should().BeEmpty();
        url.CreationTime.Should().BeEmpty();
        url.LastModifiedTime.Should().BeEmpty();
    }

    [Fact]
    public void ToEventSourceMapping_WhenAllFieldsPopulated_MapsEveryValue()
    {
        // Arrange
        var configuration = new EventSourceMappingConfiguration
        {
            UUID = "11111111-2222-3333-4444-555555555555",
            EventSourceArn = "arn:aws:sqs:eu-west-1:000000000000:orders-queue",
            FunctionArn = "arn:aws:lambda:eu-west-1:000000000000:function:process-orders",
            State = "Enabled",
            BatchSize = 10,
            LastModified = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
        };

        // Act
        var mapping = LambdaFunctionMapper.ToEventSourceMapping(configuration);

        // Assert
        mapping.Uuid.Should().Be("11111111-2222-3333-4444-555555555555");
        mapping.EventSourceArn.Should().Be("arn:aws:sqs:eu-west-1:000000000000:orders-queue");
        mapping.FunctionArn.Should().Be("arn:aws:lambda:eu-west-1:000000000000:function:process-orders");
        mapping.State.Should().Be("Enabled");
        mapping.BatchSize.Should().Be(10);
        mapping.LastModified.Should().Be("2026-01-02T03:04:05.0000000Z");
    }

    [Fact]
    public void ToEventSourceMapping_WhenFieldsUnset_AppliesSafeDefaults()
    {
        // Arrange
        var configuration = new EventSourceMappingConfiguration();

        // Act
        var mapping = LambdaFunctionMapper.ToEventSourceMapping(configuration);

        // Assert
        mapping.Uuid.Should().BeEmpty();
        mapping.EventSourceArn.Should().BeEmpty();
        mapping.FunctionArn.Should().BeEmpty();
        mapping.State.Should().BeEmpty();
        mapping.BatchSize.Should().Be(0);
        mapping.LastModified.Should().BeEmpty();
    }

    [Fact]
    public void ToLogEvent_WhenAllFieldsPopulated_MapsEveryValue()
    {
        // Arrange
        var logEvent = new FilteredLogEvent
        {
            Timestamp = 1_700_000_000_000,
            Message = "START RequestId: abc",
            LogStreamName = "2026/01/02/[$LATEST]abcdef",
        };

        // Act
        var mapped = LambdaFunctionMapper.ToLogEvent(logEvent);

        // Assert
        mapped.Timestamp.Should().Be("2023-11-14T22:13:20.0000000+00:00");
        mapped.Message.Should().Be("START RequestId: abc");
        mapped.LogStreamName.Should().Be("2026/01/02/[$LATEST]abcdef");
    }

    [Fact]
    public void ToLogEvent_WhenFieldsUnset_AppliesSafeDefaults()
    {
        // Arrange
        var logEvent = new FilteredLogEvent();

        // Act
        var mapped = LambdaFunctionMapper.ToLogEvent(logEvent);

        // Assert
        mapped.Timestamp.Should().BeEmpty();
        mapped.Message.Should().BeEmpty();
        mapped.LogStreamName.Should().BeEmpty();
    }

    [Fact]
    public void ToLayer_WhenArnIsFullyQualified_DerivesNameAndVersion()
    {
        // Arrange
        var layer = new Layer { Arn = "arn:aws:lambda:eu-west-1:123456789012:layer:shared-utils:7" };

        // Act
        var mapped = LambdaFunctionMapper.ToLayer(layer);

        // Assert
        mapped.Arn.Should().Be("arn:aws:lambda:eu-west-1:123456789012:layer:shared-utils:7");
        mapped.Name.Should().Be("shared-utils");
        mapped.Version.Should().Be("7");
    }

    [Fact]
    public void ToLayer_WhenArnIsUnsetOrIncomplete_AppliesSafeDefaults()
    {
        // Arrange
        var layer = new Layer();

        // Act
        var mapped = LambdaFunctionMapper.ToLayer(layer);

        // Assert
        mapped.Arn.Should().BeEmpty();
        mapped.Name.Should().BeEmpty();
        mapped.Version.Should().BeEmpty();
    }
}
