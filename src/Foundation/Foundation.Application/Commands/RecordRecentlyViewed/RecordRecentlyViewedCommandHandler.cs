using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Preferences;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.RecordRecentlyViewed;

internal sealed partial class RecordRecentlyViewedCommandHandler : ICommandHandler<RecordRecentlyViewedCommand>
{
    private const int MaxRecentlyViewed = 10;

    private readonly IUserDataStore _userDataStore;
    private readonly ILogger _logger;

    public RecordRecentlyViewedCommandHandler(IUserDataStore userDataStore, ILogger<RecordRecentlyViewedCommandHandler> logger)
    {
        _userDataStore = userDataStore;
        _logger = logger;
    }

    public async Task<Result> Handle(RecordRecentlyViewedCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Reference);
        var preferences = await _userDataStore.GetPreferencesAsync(cancellationToken);
        if (!preferences.IsSuccess)
            return preferences.Error!.Value;

        var updated = preferences.Value.WithRecentlyViewed(request.Reference, MaxRecentlyViewed);
        return await _userDataStore.SavePreferencesAsync(updated, cancellationToken);
    }

    [LoggerMessage(LogLevel.Trace, "Recording recently-viewed resource {Reference}.")]
    private partial void LogHandling(string reference);
}
