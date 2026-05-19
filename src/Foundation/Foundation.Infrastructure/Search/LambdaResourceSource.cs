using Foundation.Application.Lambda;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes Lambda functions to the global search index. Failures are swallowed and reported as
/// an empty list so a Lambda backend that is unavailable or unsupported cannot abort a full
/// index rebuild.
/// </summary>
internal sealed class LambdaResourceSource : IResourceSource
{
    private readonly ILambdaClient _client;

    public LambdaResourceSource(ILambdaClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "lambda";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var functions = await _client.ListFunctionsAsync(cancellationToken);
        if (!functions.IsSuccess)
        {
            return [];
        }

        return functions.Value
            .Select(function => new SearchEntry(
                ServiceKey,
                function.FunctionName,
                function.FunctionName,
                $"/services/lambda/{function.FunctionName}"))
            .ToList();
    }
}
