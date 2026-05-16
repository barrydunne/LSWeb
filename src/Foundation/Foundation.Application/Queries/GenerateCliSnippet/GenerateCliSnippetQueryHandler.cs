using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Configuration;
using Foundation.Domain.Snippets;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GenerateCliSnippet;

internal sealed partial class GenerateCliSnippetQueryHandler
    : IQueryHandler<GenerateCliSnippetQuery, GenerateCliSnippetQueryResult>
{
    private readonly IConfigProvider _configProvider;
    private readonly ILogger _logger;

    public GenerateCliSnippetQueryHandler(IConfigProvider configProvider, ILogger<GenerateCliSnippetQueryHandler> logger)
    {
        _configProvider = configProvider;
        _logger = logger;
    }

    public Task<Result<GenerateCliSnippetQueryResult>> Handle(GenerateCliSnippetQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Service, request.Operation);

        var snapshot = _configProvider.GetSnapshot();
        var context = new CliConnectionContext(snapshot.ServiceUrl.Value, snapshot.Region.Value);
        var operation = new CliOperation(request.Service, request.Operation, request.Parameters);
        var snippet = CliSnippetGenerator.Generate(operation, context);

        LogHandled();

        return Task.FromResult<Result<GenerateCliSnippetQueryResult>>(
            new GenerateCliSnippetQueryResult(snippet.Command));
    }

    [LoggerMessage(LogLevel.Trace, "Generating CLI snippet for {Service} {Operation}.")]
    private partial void LogHandling(string service, string operation);

    [LoggerMessage(LogLevel.Trace, "CLI snippet generated.")]
    private partial void LogHandled();
}
