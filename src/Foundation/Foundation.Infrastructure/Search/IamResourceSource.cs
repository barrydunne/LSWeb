using Foundation.Application.Iam;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes IAM users, groups, roles and customer-managed policies to the global search index.
/// Each resource type is listed independently so a single failing call degrades to skipping just
/// that type rather than aborting the whole IAM contribution; a fully unavailable IAM backend
/// simply yields an empty list and never aborts a full index rebuild.
/// </summary>
internal sealed class IamResourceSource : IResourceSource
{
    private readonly IIamClient _client;

    public IamResourceSource(IIamClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "iam";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var entries = new List<SearchEntry>();

        var users = await _client.ListUsersAsync(cancellationToken);
        if (users.IsSuccess)
        {
            entries.AddRange(users.Value.Select(user => CreateEntry("user", user.UserName)));
        }

        var groups = await _client.ListGroupsAsync(cancellationToken);
        if (groups.IsSuccess)
        {
            entries.AddRange(groups.Value.Select(group => CreateEntry("group", group.GroupName)));
        }

        var roles = await _client.ListRolesAsync(cancellationToken);
        if (roles.IsSuccess)
        {
            entries.AddRange(roles.Value.Select(role => CreateEntry("role", role.RoleName)));
        }

        var policies = await _client.ListPoliciesAsync(false, cancellationToken);
        if (policies.IsSuccess)
        {
            entries.AddRange(policies.Value.Select(policy => CreateEntry("policy", policy.PolicyName)));
        }

        return entries;
    }

    /// <summary>
    /// Builds a search entry for a single IAM resource. The resource id carries the type prefix
    /// (for example <c>role/MyRole</c>) so the IAM detail view can dispatch by type and so results
    /// remain unique when a user and a role share a name; the display name stays the bare name.
    /// </summary>
    private SearchEntry CreateEntry(string type, string name)
        => new(
            ServiceKey,
            $"{type}/{name}",
            name,
            $"/services/{ServiceKey}/{type}/{Uri.EscapeDataString(name)}");
}
