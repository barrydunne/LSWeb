using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Queries.ListLambdaEventSourceMappings;

/// <summary>
/// List the event source mappings configured for a Lambda function.
/// </summary>
/// <param name="FunctionName">The name of the function whose event source mappings to list.</param>
public record ListLambdaEventSourceMappingsQuery(string FunctionName) : IQuery<ListLambdaEventSourceMappingsQueryResult>;

/// <summary>
/// The event source mappings configured for a Lambda function.
/// </summary>
/// <param name="Mappings">The event source mappings, ordered by their source ARN.</param>
/// <param name="S3Triggers">The S3 buckets configured to trigger the function, ordered by their bucket ARN.</param>
public record ListLambdaEventSourceMappingsQueryResult(
    IReadOnlyList<LambdaEventSourceMapping> Mappings,
    IReadOnlyList<LambdaS3Trigger> S3Triggers);
