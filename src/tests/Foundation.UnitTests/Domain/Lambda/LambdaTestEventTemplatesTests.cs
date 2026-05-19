using Foundation.Domain.Lambda;

namespace Foundation.UnitTests.Domain.Lambda;

public class LambdaTestEventTemplatesTests
{
    [Fact]
    public void Templates_ContainsTheExpectedStarterEvents()
    {
        // Act
        var templates = LambdaTestEventTemplates.Templates;

        // Assert
        templates.Should().NotBeEmpty();
        templates.Select(_ => _.Name).Should().BeEquivalentTo(
            "Empty",
            "API Gateway (HTTP)",
            "S3 Put",
            "SQS Message",
            "SNS Notification",
            "Scheduled (EventBridge)");
    }

    [Fact]
    public void Templates_EveryTemplateHasNameAndPayload()
    {
        // Act
        var templates = LambdaTestEventTemplates.Templates;

        // Assert
        templates.Should().OnlyContain(_ => !string.IsNullOrWhiteSpace(_.Name) && !string.IsNullOrWhiteSpace(_.Payload));
    }

    [Fact]
    public void Templates_EmptyTemplateIsAnEmptyJsonObject()
    {
        // Act
        var empty = LambdaTestEventTemplates.Templates.Single(_ => _.Name == "Empty");

        // Assert
        empty.Payload.Should().Be("{}");
    }
}
