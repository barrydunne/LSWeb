using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Ssm;

namespace Foundation.Application.Queries.BrowseParameters;

/// <summary>
/// Browse the SSM parameters that live under a hierarchy path, optionally descending into the full
/// tree beneath it.
/// </summary>
/// <param name="Path">The hierarchy path to browse, such as <c>/</c> or <c>/app/config</c>.</param>
/// <param name="Recursive">Whether to include parameters in nested paths beneath the path.</param>
public record BrowseParametersQuery(string Path, bool Recursive) : IQuery<BrowseParametersQueryResult>;

/// <summary>
/// The SSM parameters that live under the requested hierarchy path.
/// </summary>
/// <param name="Path">The hierarchy path that was browsed.</param>
/// <param name="Parameters">The parameters under the path, ordered as returned by the backend.</param>
public record BrowseParametersQueryResult(string Path, IReadOnlyList<Parameter> Parameters);
