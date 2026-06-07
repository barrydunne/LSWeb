using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Seed;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetSeedTemplates;

internal sealed partial class GetSeedTemplatesQueryHandler : IQueryHandler<GetSeedTemplatesQuery, GetSeedTemplatesQueryResult>
{
    private readonly ISeedTemplateCatalogue _catalogue;
    private readonly ILogger _logger;

    public GetSeedTemplatesQueryHandler(ISeedTemplateCatalogue catalogue, ILogger<GetSeedTemplatesQueryHandler> logger)
    {
        _catalogue = catalogue;
        _logger = logger;
    }

    public Task<Result<GetSeedTemplatesQueryResult>> Handle(GetSeedTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = _catalogue.GetTemplates();
        LogHandling(templates.Count);
        Result<GetSeedTemplatesQueryResult> result = new GetSeedTemplatesQueryResult(templates);
        return Task.FromResult(result);
    }

    [LoggerMessage(LogLevel.Trace, "Handling seed templates query. Returning {Count} template(s).")]
    private partial void LogHandling(int count);
}
