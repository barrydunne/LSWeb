using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Preferences;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.AddFavourite;

internal sealed partial class AddFavouriteCommandHandler : ICommandHandler<AddFavouriteCommand>
{
    private readonly IUserDataStore _userDataStore;
    private readonly ILogger _logger;

    public AddFavouriteCommandHandler(IUserDataStore userDataStore, ILogger<AddFavouriteCommandHandler> logger)
    {
        _userDataStore = userDataStore;
        _logger = logger;
    }

    public async Task<Result> Handle(AddFavouriteCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Reference);
        var preferences = await _userDataStore.GetPreferencesAsync(cancellationToken);
        if (!preferences.IsSuccess)
            return preferences.Error!.Value;

        var updated = preferences.Value.WithFavourite(request.Reference);
        return await _userDataStore.SavePreferencesAsync(updated, cancellationToken);
    }

    [LoggerMessage(LogLevel.Trace, "Pinning favourite resource {Reference}.")]
    private partial void LogHandling(string reference);
}
