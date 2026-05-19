using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteLambdaTestEvent;

internal sealed partial class DeleteLambdaTestEventCommandHandler : ICommandHandler<DeleteLambdaTestEventCommand>
{
    private readonly ITestEventStore _store;
    private readonly ILogger _logger;

    public DeleteLambdaTestEventCommandHandler(ITestEventStore store, ILogger<DeleteLambdaTestEventCommandHandler> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteLambdaTestEventCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName, request.Name);
        var result = await _store.DeleteEventAsync(request.FunctionName, request.Name, cancellationToken);
        LogHandled(result.IsSuccess);
        return result;
    }

    [LoggerMessage(LogLevel.Trace, "Deleting Lambda test event {Name} from {FunctionName}.")]
    private partial void LogHandling(string functionName, string name);

    [LoggerMessage(LogLevel.Trace, "Lambda test event deletion handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
