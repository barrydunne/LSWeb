namespace Foundation.Domain.Catalogue;

/// <summary>
/// Groups managed AWS services into a functional category for display.
/// </summary>
public enum ServiceCategory
{
    /// <summary>
    /// Compute and orchestration services (for example Lambda, Step Functions).
    /// </summary>
    Compute,

    /// <summary>
    /// Object and file storage services (for example S3).
    /// </summary>
    Storage,

    /// <summary>
    /// Database services (for example DynamoDB).
    /// </summary>
    Database,

    /// <summary>
    /// Messaging and integration services (for example SNS, SQS).
    /// </summary>
    Messaging,

    /// <summary>
    /// Security, identity and secrets services (for example Secrets Manager).
    /// </summary>
    Security,

    /// <summary>
    /// Management and configuration services (for example SSM Parameter Store).
    /// </summary>
    Management,

    /// <summary>
    /// Monitoring and observability services (for example CloudWatch Logs).
    /// </summary>
    Monitoring,
}
