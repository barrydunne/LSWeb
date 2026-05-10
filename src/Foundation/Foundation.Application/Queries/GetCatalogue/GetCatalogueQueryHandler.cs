using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Capabilities;
using Foundation.Domain.Catalogue;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetCatalogue;

internal sealed partial class GetCatalogueQueryHandler : IQueryHandler<GetCatalogueQuery, GetCatalogueQueryResult>
{
    private readonly ICapabilityProvider _capabilityProvider;
    private readonly ILogger _logger;

    public GetCatalogueQueryHandler(ICapabilityProvider capabilityProvider, ILogger<GetCatalogueQueryHandler> logger)
    {
        _capabilityProvider = capabilityProvider;
        _logger = logger;
    }

    public Task<Result<GetCatalogueQueryResult>> Handle(GetCatalogueQuery request, CancellationToken cancellationToken)
    {
        LogHandling(ServiceCatalogue.Services.Count);
        Result<GetCatalogueQueryResult> result = new GetCatalogueQueryResult(
            ServiceCatalogue.Services,
            _capabilityProvider.GetCapabilities());
        return Task.FromResult(result);
    }

    [LoggerMessage(LogLevel.Trace, "Handling catalogue query. Returning {Count} services.")]
    private partial void LogHandling(int count);
}
