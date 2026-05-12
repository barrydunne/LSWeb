namespace Foundation.Domain.Streaming;

/// <summary>
/// The lifecycle state of a backend operation as it is reported to connected clients.
/// </summary>
public enum OperationState
{
    /// <summary>
    /// The operation has started and is still running.
    /// </summary>
    InProgress,

    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The operation finished with a failure.
    /// </summary>
    Failed,
}
