namespace Foundation.Domain.CloudFormation;

/// <summary>
/// The status of a CloudFormation drift detection operation, as reported while it runs and once it
/// completes.
/// </summary>
/// <param name="StackDriftDetectionId">The id that identifies the drift detection operation.</param>
/// <param name="StackId">The Amazon Resource Name of the stack the detection ran against.</param>
/// <param name="DetectionStatus">The detection status, for example <c>DETECTION_IN_PROGRESS</c>, <c>DETECTION_COMPLETE</c>, or <c>DETECTION_FAILED</c>.</param>
/// <param name="DetectionStatusReason">The reason the detection reached its status, where the backend reports one.</param>
/// <param name="StackDriftStatusValue">The overall stack drift status, for example <c>IN_SYNC</c>, <c>DRIFTED</c>, or <c>UNKNOWN</c>.</param>
/// <param name="DriftedStackResourceCount">The number of resources that have drifted, where the detection has counted them.</param>
/// <param name="Timestamp">The time the detection status was last updated.</param>
public sealed record StackDriftStatus(
    string StackDriftDetectionId,
    string StackId,
    string DetectionStatus,
    string? DetectionStatusReason,
    string StackDriftStatusValue,
    int DriftedStackResourceCount,
    DateTimeOffset Timestamp);
