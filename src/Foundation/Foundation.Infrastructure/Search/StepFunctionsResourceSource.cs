using Foundation.Application.StepFunctions;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes Step Functions state machines to the global search index. Failures are swallowed and
/// reported as an empty list so a backend that is unavailable or unsupported cannot abort a full
/// index rebuild.
/// </summary>
internal sealed class StepFunctionsResourceSource : IResourceSource
{
    private readonly IStepFunctionsClient _client;

    public StepFunctionsResourceSource(IStepFunctionsClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "step-functions";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var stateMachines = await _client.ListStateMachinesAsync(cancellationToken);
        if (!stateMachines.IsSuccess)
        {
            return [];
        }

        return stateMachines.Value
            .Select(stateMachine => new SearchEntry(
                ServiceKey,
                stateMachine.Name,
                stateMachine.Name,
                $"/services/step-functions/{Uri.EscapeDataString(stateMachine.StateMachineArn)}"))
            .ToList();
    }
}
