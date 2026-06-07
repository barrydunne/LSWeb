using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Application.Commands.CreateLogGroup;
using Foundation.Application.Commands.CreateParameter;
using Foundation.Application.Commands.CreateS3Bucket;
using Foundation.Application.Commands.CreateSnsTopic;
using Foundation.Application.Commands.CreateSqsQueue;
using Foundation.Domain.Seed;

namespace Foundation.Application.Seed;

/// <summary>
/// The built-in catalogue of seed templates. Each template is expressed as a plan whose steps reuse
/// the existing service commands, so seeding goes through the same adapters and notifications as any
/// other resource creation.
/// </summary>
internal sealed class SeedTemplateCatalogue : ISeedTemplateCatalogue
{
    private static readonly IReadOnlyList<SeedTemplatePlan> _plans =
    [
        Plan(
            "messaging-starter",
            "Messaging starter",
            "An SQS queue and an SNS topic for event-driven messaging.",
            Step("sqs", "Queue", "seed-orders-queue", new CreateSqsQueueCommand("seed-orders-queue", false)),
            Step("sns", "Topic", "seed-orders-topic", new CreateSnsTopicCommand("seed-orders-topic"))),
        Plan(
            "storage-starter",
            "Storage starter",
            "An S3 bucket and an SSM parameter for application configuration.",
            Step("s3", "Bucket", "seed-app-assets", new CreateS3BucketCommand("seed-app-assets")),
            Step(
                "ssm-parameter-store",
                "Parameter",
                "/seed/app/config",
                new CreateParameterCommand("/seed/app/config", "String", "sample-value", "Seeded sample parameter."))),
        Plan(
            "observability-starter",
            "Observability starter",
            "A CloudWatch log group for capturing application logs.",
            Step("cloudwatch-logs", "Log group", "/seed/app/logs", new CreateLogGroupCommand("/seed/app/logs"))),
    ];

    /// <inheritdoc />
    public IReadOnlyList<SeedTemplate> GetTemplates()
        => _plans.Select(plan => plan.Template).ToList();

    /// <inheritdoc />
    public SeedTemplatePlan? GetPlan(string templateId)
        => _plans.FirstOrDefault(plan => plan.Template.Id == templateId);

    private static SeedTemplatePlan Plan(string id, string name, string description, params SeedActionStep[] steps)
        => new(
            new SeedTemplate(id, name, description, steps.Select(step => step.Descriptor).ToList()),
            steps);

    private static SeedActionStep Step(string serviceKey, string resourceType, string name, ICommand command)
        => new(new SeedResourceDescriptor(serviceKey, resourceType, name), command);
}
