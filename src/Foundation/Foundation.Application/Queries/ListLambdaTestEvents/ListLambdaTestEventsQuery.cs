using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Queries.ListLambdaTestEvents;

/// <summary>
/// List the saved test events for a Lambda function together with the starter templates available
/// to seed new events.
/// </summary>
/// <param name="FunctionName">The name of the function whose test events to list.</param>
public record ListLambdaTestEventsQuery(string FunctionName) : IQuery<ListLambdaTestEventsQueryResult>;

/// <summary>
/// The saved test events for a function and the starter templates that can seed new events.
/// </summary>
/// <param name="Events">The events the user has saved for the function, ordered by name.</param>
/// <param name="Templates">The fixed starter templates available to all functions.</param>
public record ListLambdaTestEventsQueryResult(
    IReadOnlyList<LambdaTestEvent> Events,
    IReadOnlyList<LambdaTestEvent> Templates);
