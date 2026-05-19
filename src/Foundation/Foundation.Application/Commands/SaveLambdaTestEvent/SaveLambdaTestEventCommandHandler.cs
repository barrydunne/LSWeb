using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SaveLambdaTestEvent;

internal sealed partial class SaveLambdaTestEventCommandHandler : ICommandHandler<SaveLambdaTestEventCommand>
{
    private readonly ITestEventStore _store;
    private readonly ILogger _logger;

    public SaveLambdaTestEventCommandHandler(ITestEventStore store, ILogger<SaveLambdaTestEventCommandHandler> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task<Result> Handle(SaveLambdaTestEventCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName, request.Name);
        var result = await _store.SaveEventAsync(
            request.FunctionName, new LambdaTestEvent(request.Name, request.Payload), cancellationToken);
        LogHandled(result.IsSuccess);
        return result;
    }

    [LoggerMessage(LogLevel.Trace, "Saving Lambda test event {Name} for {FunctionName}.")]
    private partial void LogHandling(string functionName, string name);

    [LoggerMessage(LogLevel.Trace, "Lambda test event save handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
