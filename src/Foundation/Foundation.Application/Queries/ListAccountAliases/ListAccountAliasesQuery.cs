using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.ListAccountAliases;

/// <summary>
/// List the account aliases configured on the backend.
/// </summary>
public record ListAccountAliasesQuery : IQuery<ListAccountAliasesQueryResult>;

/// <summary>
/// The account aliases configured on the backend.
/// </summary>
/// <param name="Aliases">The account aliases, ordered as returned by the backend.</param>
public record ListAccountAliasesQueryResult(IReadOnlyList<string> Aliases);
