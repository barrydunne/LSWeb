using Foundation.Application.Commands.CreateLogGroup;
using Foundation.Application.Commands.CreateParameter;
using Foundation.Application.Commands.CreateS3Bucket;
using Foundation.Application.Commands.CreateSnsTopic;
using Foundation.Application.Commands.CreateSqsQueue;
using Foundation.Application.Seed;
using Foundation.Domain.Seed;

namespace Foundation.UnitTests.Application.Seed;

public class SeedTemplateCatalogueTests
{
    private readonly SeedTemplateCatalogue _sut = new();

    [Fact]
    public void GetTemplates_ReturnsTheExpectedCatalogue()
    {
        // Act
        var templates = _sut.GetTemplates();

        // Assert
        templates.Select(template => template.Id).Should().Equal(
            "messaging-starter", "storage-starter", "observability-starter");

        var messaging = templates.Single(template => template.Id == "messaging-starter");
        messaging.Name.Should().Be("Messaging starter");
        messaging.Description.Should().Be("An SQS queue and an SNS topic for event-driven messaging.");
        messaging.Resources.Should().BeEquivalentTo(
            [
                new SeedResourceDescriptor("sqs", "Queue", "seed-orders-queue"),
                new SeedResourceDescriptor("sns", "Topic", "seed-orders-topic"),
            ],
            options => options.WithStrictOrdering());

        var storage = templates.Single(template => template.Id == "storage-starter");
        storage.Name.Should().Be("Storage starter");
        storage.Description.Should().Be("An S3 bucket and an SSM parameter for application configuration.");
        storage.Resources.Should().BeEquivalentTo(
            [
                new SeedResourceDescriptor("s3", "Bucket", "seed-app-assets"),
                new SeedResourceDescriptor("ssm-parameter-store", "Parameter", "/seed/app/config"),
            ],
            options => options.WithStrictOrdering());

        var observability = templates.Single(template => template.Id == "observability-starter");
        observability.Name.Should().Be("Observability starter");
        observability.Description.Should().Be("A CloudWatch log group for capturing application logs.");
        observability.Resources.Should().BeEquivalentTo(
            [new SeedResourceDescriptor("cloudwatch-logs", "Log group", "/seed/app/logs")]);
    }

    [Fact]
    public void GetPlan_WhenTemplateKnown_ReturnsPlanWithMatchingCommands()
    {
        // Act
        var messaging = _sut.GetPlan("messaging-starter");
        var storage = _sut.GetPlan("storage-starter");
        var observability = _sut.GetPlan("observability-starter");

        // Assert
        messaging!.Steps.Select(step => step.Command).Should().SatisfyRespectively(
            command => command.Should().BeEquivalentTo(new CreateSqsQueueCommand("seed-orders-queue", false)),
            command => command.Should().BeEquivalentTo(new CreateSnsTopicCommand("seed-orders-topic")));

        storage!.Steps.Select(step => step.Command).Should().SatisfyRespectively(
            command => command.Should().BeEquivalentTo(new CreateS3BucketCommand("seed-app-assets")),
            command => command.Should().BeEquivalentTo(
                new CreateParameterCommand("/seed/app/config", "String", "sample-value", "Seeded sample parameter.")));

        observability!.Steps.Select(step => step.Command).Should().SatisfyRespectively(
            command => command.Should().BeEquivalentTo(new CreateLogGroupCommand("/seed/app/logs")));
    }

    [Fact]
    public void GetPlan_WhenTemplateUnknown_ReturnsNull()
    {
        // Act
        var plan = _sut.GetPlan("does-not-exist");

        // Assert
        plan.Should().BeNull();
    }
}
