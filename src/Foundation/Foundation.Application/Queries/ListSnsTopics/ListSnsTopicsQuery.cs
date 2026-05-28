using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Sns;

namespace Foundation.Application.Queries.ListSnsTopics;

/// <summary>
/// List the SNS topics available on the backend.
/// </summary>
public record ListSnsTopicsQuery : IQuery<ListSnsTopicsQueryResult>;

/// <summary>
/// The SNS topics available on the backend.
/// </summary>
/// <param name="Topics">The topics, ordered as returned by the backend.</param>
public record ListSnsTopicsQueryResult(IReadOnlyList<SnsTopic> Topics);
