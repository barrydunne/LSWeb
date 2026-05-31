namespace Foundation.Domain.Catalogue;

/// <summary>
/// The built-in set of AWS services the console manages.
/// </summary>
public static class ServiceCatalogue
{
    /// <summary>
    /// Gets the managed services in display order. Each service has a unique key and route.
    /// </summary>
    public static IReadOnlyList<ServiceDescriptor> Services { get; } =
    [
        new("cloudwatch-logs", "CloudWatch Logs", ServiceCategory.Monitoring, "log", "/services/cloudwatch-logs"),
        new("dynamodb", "DynamoDB", ServiceCategory.Database, "database", "/services/dynamodb"),
        new("iam", "IAM", ServiceCategory.Security, "shield", "/services/iam"),
        new("lambda", "Lambda", ServiceCategory.Compute, "zap", "/services/lambda"),
        new("s3", "S3", ServiceCategory.Storage, "archive", "/services/s3"),
        new("secrets-manager", "Secrets Manager", ServiceCategory.Security, "key", "/services/secrets-manager"),
        new("sns", "SNS", ServiceCategory.Messaging, "broadcast", "/services/sns"),
        new("sqs", "SQS", ServiceCategory.Messaging, "inbox", "/services/sqs"),
        new("ssm-parameter-store", "SSM Parameter Store", ServiceCategory.Management, "gear", "/services/ssm-parameter-store"),
        new("step-functions", "Step Functions", ServiceCategory.Compute, "workflow", "/services/step-functions"),
    ];
}
