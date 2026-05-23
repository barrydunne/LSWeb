using System.Text;
using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.S3;
using Foundation.Domain.S3;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.PreviewS3Object;

internal sealed partial class PreviewS3ObjectQueryHandler : IQueryHandler<PreviewS3ObjectQuery, PreviewS3ObjectQueryResult>
{
    private const int MaxPreviewBytes = 256 * 1024;

    private readonly IS3Client _client;
    private readonly ILogger _logger;

    public PreviewS3ObjectQueryHandler(IS3Client client, ILogger<PreviewS3ObjectQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<PreviewS3ObjectQueryResult>> Handle(PreviewS3ObjectQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName, request.Key);
        var preview = await _client.PreviewObjectAsync(request.BucketName, request.Key, MaxPreviewBytes, cancellationToken);
        LogHandled(preview.IsSuccess);

        if (!preview.IsSuccess)
        {
            Result<PreviewS3ObjectQueryResult> failure = preview.Error!.Value;
            return failure;
        }

        var content = preview.Value;
        var kind = S3PreviewClassifier.Classify(content.ContentType, request.Key);
        return BuildResult(kind, content);
    }

    private static PreviewS3ObjectQueryResult BuildResult(S3PreviewKind kind, S3ObjectPreview content)
    {
        string? text = null;
        string? dataUrl = null;

        if (kind is S3PreviewKind.Text or S3PreviewKind.Json)
            text = Encoding.UTF8.GetString(content.Content);
        else if (kind == S3PreviewKind.Image)
            dataUrl = $"data:{content.ContentType};base64,{Convert.ToBase64String(content.Content)}";

        return new PreviewS3ObjectQueryResult(
            kind.ToString(), content.ContentType, content.Truncated, content.TotalSize, text, dataUrl);
    }

    [LoggerMessage(LogLevel.Trace, "Previewing S3 object {Key} from {BucketName}.")]
    private partial void LogHandling(string bucketName, string key);

    [LoggerMessage(LogLevel.Trace, "S3 object preview handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
