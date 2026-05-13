using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Navigation;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ResolveReference;

internal sealed partial class ResolveReferenceQueryHandler : IQueryHandler<ResolveReferenceQuery, ResolveReferenceQueryResult>
{
    private readonly IReferenceResolver _resolver;
    private readonly ILogger _logger;

    public ResolveReferenceQueryHandler(IReferenceResolver resolver, ILogger<ResolveReferenceQueryHandler> logger)
    {
        _resolver = resolver;
        _logger = logger;
    }

    public Task<Result<ResolveReferenceQueryResult>> Handle(ResolveReferenceQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Reference);
        var resolution = _resolver.Resolve(request.Reference, request.Service);
        LogHandled(resolution.IsSuccess);

        if (!resolution.IsSuccess)
        {
            Result<ResolveReferenceQueryResult> failure = resolution.Error!.Value;
            return Task.FromResult(failure);
        }

        var reference = resolution.Value;
        Result<ResolveReferenceQueryResult> success =
            new ResolveReferenceQueryResult(reference.ServiceKey, reference.ResourceId, reference.Route);
        return Task.FromResult(success);
    }

    [LoggerMessage(LogLevel.Trace, "Resolving reference {Reference}.")]
    private partial void LogHandling(string reference);

    [LoggerMessage(LogLevel.Trace, "Reference resolution handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
