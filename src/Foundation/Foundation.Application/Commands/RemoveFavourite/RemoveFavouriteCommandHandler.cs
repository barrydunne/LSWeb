using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Preferences;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.RemoveFavourite;

internal sealed partial class RemoveFavouriteCommandHandler : ICommandHandler<RemoveFavouriteCommand>
{
    private readonly IUserDataStore _userDataStore;
    private readonly ILogger _logger;

    public RemoveFavouriteCommandHandler(IUserDataStore userDataStore, ILogger<RemoveFavouriteCommandHandler> logger)
    {
        _userDataStore = userDataStore;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveFavouriteCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Reference);
        var preferences = await _userDataStore.GetPreferencesAsync(cancellationToken);
        if (!preferences.IsSuccess)
            return preferences.Error!.Value;

        var updated = preferences.Value.WithoutFavourite(request.Reference);
        return await _userDataStore.SavePreferencesAsync(updated, cancellationToken);
    }

    [LoggerMessage(LogLevel.Trace, "Unpinning favourite resource {Reference}.")]
    private partial void LogHandling(string reference);
}
