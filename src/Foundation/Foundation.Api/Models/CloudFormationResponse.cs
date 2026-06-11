namespace Foundation.Api.Models;

/// <summary>
/// The CloudFormation stacks available on the backend.
/// </summary>
/// <param name="Stacks">The stack summaries, ordered as returned by the backend.</param>
public sealed record CloudFormationStackListResponse(
    IReadOnlyList<CloudFormationStackSummaryResponse> Stacks);

/// <summary>
/// A concise view of a CloudFormation stack as it appears in a list.
/// </summary>
/// <param name="StackName">The stack name.</param>
/// <param name="StackId">The Amazon Resource Name that uniquely identifies the stack.</param>
/// <param name="StackStatus">The stack status, for example <c>CREATE_COMPLETE</c> or <c>UPDATE_COMPLETE</c>.</param>
/// <param name="Description">The stack description, where one applies.</param>
/// <param name="CreationTime">The moment the stack was created.</param>
/// <param name="LastUpdatedTime">The moment the stack was last updated, or <see langword="null"/> when it has never been updated.</param>
public sealed record CloudFormationStackSummaryResponse(
    string StackName,
    string StackId,
    string StackStatus,
    string? Description,
    DateTimeOffset CreationTime,
    DateTimeOffset? LastUpdatedTime);

/// <summary>
/// The full details of a CloudFormation stack.
/// </summary>
/// <param name="StackName">The stack name.</param>
/// <param name="StackId">The Amazon Resource Name that uniquely identifies the stack.</param>
/// <param name="StackStatus">The stack status, for example <c>CREATE_COMPLETE</c> or <c>UPDATE_COMPLETE</c>.</param>
/// <param name="StackStatusReason">The reason for the current status, where one applies.</param>
/// <param name="Description">The stack description, where one applies.</param>
/// <param name="CreationTime">The moment the stack was created.</param>
/// <param name="LastUpdatedTime">The moment the stack was last updated, or <see langword="null"/> when it has never been updated.</param>
/// <param name="Parameters">The input parameters supplied to the stack.</param>
/// <param name="Outputs">The output values exported by the stack.</param>
/// <param name="Tags">The tags applied to the stack.</param>
/// <param name="Capabilities">The capabilities acknowledged for the stack.</param>
public sealed record CloudFormationStackDetailResponse(
    string StackName,
    string StackId,
    string StackStatus,
    string? StackStatusReason,
    string? Description,
    DateTimeOffset CreationTime,
    DateTimeOffset? LastUpdatedTime,
    IReadOnlyList<StackParameterResponse> Parameters,
    IReadOnlyList<StackOutputResponse> Outputs,
    IReadOnlyList<StackTagResponse> Tags,
    IReadOnlyList<string> Capabilities);

/// <summary>
/// An input parameter supplied to a CloudFormation stack.
/// </summary>
/// <param name="ParameterKey">The parameter name.</param>
/// <param name="ParameterValue">The parameter value.</param>
public sealed record StackParameterResponse(
    string ParameterKey,
    string ParameterValue);

/// <summary>
/// An output value exported by a CloudFormation stack.
/// </summary>
/// <param name="OutputKey">The output name.</param>
/// <param name="OutputValue">The output value.</param>
/// <param name="Description">The output description, where one applies.</param>
/// <param name="ExportName">The cross-stack export name, where one applies.</param>
public sealed record StackOutputResponse(
    string OutputKey,
    string OutputValue,
    string? Description,
    string? ExportName);

/// <summary>
/// A tag applied to a CloudFormation stack.
/// </summary>
/// <param name="Key">The tag key.</param>
/// <param name="Value">The tag value.</param>
public sealed record StackTagResponse(
    string Key,
    string Value);

/// <summary>
/// The template that defines a CloudFormation stack.
/// </summary>
/// <param name="TemplateBody">The raw template body as stored by the backend.</param>
/// <param name="Format">The detected template format, either <c>json</c> or <c>yaml</c>.</param>
public sealed record CloudFormationStackTemplateResponse(
    string TemplateBody,
    string Format);

/// <summary>
/// The resources a CloudFormation stack manages.
/// </summary>
/// <param name="Resources">The stack resources, ordered as returned by the backend.</param>
public sealed record CloudFormationStackResourceListResponse(
    IReadOnlyList<CloudFormationStackResourceResponse> Resources);

/// <summary>
/// A resource a CloudFormation stack manages.
/// </summary>
/// <param name="LogicalResourceId">The logical id the template assigns to the resource.</param>
/// <param name="PhysicalResourceId">The physical id of the provisioned resource, where one exists.</param>
/// <param name="ResourceType">The CloudFormation resource type, for example <c>AWS::SQS::Queue</c>.</param>
/// <param name="ResourceStatus">The current status of the resource.</param>
/// <param name="ResourceStatusReason">The reason for the current status, where one applies.</param>
/// <param name="LastUpdatedTime">The moment the resource status was last updated.</param>
public sealed record CloudFormationStackResourceResponse(
    string LogicalResourceId,
    string? PhysicalResourceId,
    string ResourceType,
    string ResourceStatus,
    string? ResourceStatusReason,
    DateTimeOffset LastUpdatedTime);

/// <summary>
/// The events recorded for a CloudFormation stack.
/// </summary>
/// <param name="Events">The stack events, ordered as returned by the backend.</param>
public sealed record CloudFormationStackEventListResponse(
    IReadOnlyList<CloudFormationStackEventResponse> Events);

/// <summary>
/// An event recorded for a CloudFormation stack.
/// </summary>
/// <param name="EventId">The unique id of the event.</param>
/// <param name="Timestamp">The moment the event was recorded.</param>
/// <param name="LogicalResourceId">The logical id the template assigns to the resource.</param>
/// <param name="PhysicalResourceId">The physical id of the provisioned resource, where one exists.</param>
/// <param name="ResourceType">The CloudFormation resource type, for example <c>AWS::SQS::Queue</c>.</param>
/// <param name="ResourceStatus">The status of the resource at the time of the event.</param>
/// <param name="ResourceStatusReason">The reason for the status, where one applies.</param>
public sealed record CloudFormationStackEventResponse(
    string EventId,
    DateTimeOffset Timestamp,
    string LogicalResourceId,
    string? PhysicalResourceId,
    string ResourceType,
    string ResourceStatus,
    string? ResourceStatusReason);

/// <summary>
/// An input parameter supplied when creating or updating a CloudFormation stack.
/// </summary>
/// <param name="ParameterKey">The parameter name.</param>
/// <param name="ParameterValue">The parameter value.</param>
public sealed record StackParameterRequest(
    string ParameterKey,
    string ParameterValue);

/// <summary>
/// The details required to create a CloudFormation stack.
/// </summary>
/// <param name="StackName">The name of the stack to create.</param>
/// <param name="TemplateBody">The inline template body that defines the stack, or <see langword="null"/> when creating from a URL.</param>
/// <param name="TemplateUrl">The S3 URL of the template that defines the stack, or <see langword="null"/> when creating from an inline body.</param>
/// <param name="Parameters">The input parameters to deploy the stack with.</param>
/// <param name="Capabilities">The capabilities the template requires, such as <c>CAPABILITY_IAM</c>.</param>
public sealed record CloudFormationStackCreateRequest(
    string StackName,
    string? TemplateBody,
    string? TemplateUrl,
    IReadOnlyList<StackParameterRequest>? Parameters,
    IReadOnlyList<string>? Capabilities);

/// <summary>
/// The template to validate, supplied either inline or by S3 URL.
/// </summary>
/// <param name="TemplateBody">The inline template body to validate, or <see langword="null"/> when validating by URL.</param>
/// <param name="TemplateUrl">The S3 URL of the template to validate, or <see langword="null"/> when validating an inline body.</param>
public sealed record CloudFormationTemplateValidationRequest(
    string? TemplateBody,
    string? TemplateUrl);

/// <summary>
/// The outcome of validating a CloudFormation template.
/// </summary>
/// <param name="Description">The description the template documents, or an empty string when none.</param>
/// <param name="CapabilitiesReason">The reason the template requires the reported capabilities, or an empty string when none.</param>
/// <param name="Capabilities">The capabilities the template requires, such as <c>CAPABILITY_IAM</c>.</param>
/// <param name="Parameters">The input parameters the template declares.</param>
public sealed record CloudFormationTemplateValidationResponse(
    string Description,
    string CapabilitiesReason,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<CloudFormationTemplateValidationParameterResponse> Parameters);

/// <summary>
/// A single input parameter that a validated CloudFormation template declares.
/// </summary>
/// <param name="ParameterKey">The name of the parameter the template declares.</param>
/// <param name="DefaultValue">The default value the template assigns to the parameter, or an empty string when none.</param>
/// <param name="NoEcho">Whether the parameter value is masked in the console and logs.</param>
/// <param name="Description">The description the template documents for the parameter, or an empty string when none.</param>
public sealed record CloudFormationTemplateValidationParameterResponse(
    string ParameterKey,
    string DefaultValue,
    bool NoEcho,
    string Description);

/// <summary>
/// The details required to update a CloudFormation stack.
/// </summary>
/// <param name="TemplateBody">The template body that defines the stack.</param>
/// <param name="Parameters">The input parameters to deploy the stack with.</param>
/// <param name="Capabilities">The capabilities the template requires, such as <c>CAPABILITY_IAM</c>.</param>
public sealed record CloudFormationStackUpdateRequest(
    string TemplateBody,
    IReadOnlyList<StackParameterRequest>? Parameters,
    IReadOnlyList<string>? Capabilities);

/// <summary>
/// The result of creating or updating a CloudFormation stack.
/// </summary>
/// <param name="StackId">The Amazon Resource Name that uniquely identifies the stack.</param>
public sealed record CloudFormationStackOperationResponse(
    string StackId);

/// <summary>
/// The change sets associated with a CloudFormation stack.
/// </summary>
/// <param name="ChangeSets">The change-set summaries, ordered as returned by the backend.</param>
public sealed record CloudFormationChangeSetListResponse(
    IReadOnlyList<CloudFormationChangeSetSummaryResponse> ChangeSets);

/// <summary>
/// A concise view of a CloudFormation change set as it appears in a list.
/// </summary>
/// <param name="ChangeSetId">The Amazon Resource Name that uniquely identifies the change set.</param>
/// <param name="ChangeSetName">The change-set name.</param>
/// <param name="StackName">The name of the stack the change set applies to.</param>
/// <param name="Status">The change-set status, for example <c>CREATE_COMPLETE</c>.</param>
/// <param name="StatusReason">The reason for the current status, where one applies.</param>
/// <param name="ExecutionStatus">The execution status, for example <c>AVAILABLE</c>.</param>
/// <param name="Description">The change-set description, where one applies.</param>
/// <param name="CreationTime">The moment the change set was created.</param>
public sealed record CloudFormationChangeSetSummaryResponse(
    string ChangeSetId,
    string ChangeSetName,
    string StackName,
    string Status,
    string? StatusReason,
    string ExecutionStatus,
    string? Description,
    DateTimeOffset CreationTime);

/// <summary>
/// The full details of a CloudFormation change set, including the resource changes it proposes.
/// </summary>
/// <param name="ChangeSetName">The change-set name.</param>
/// <param name="ChangeSetId">The Amazon Resource Name that uniquely identifies the change set.</param>
/// <param name="StackName">The name of the stack the change set applies to.</param>
/// <param name="StackId">The Amazon Resource Name that uniquely identifies the stack.</param>
/// <param name="Status">The change-set status, for example <c>CREATE_COMPLETE</c>.</param>
/// <param name="StatusReason">The reason for the current status, where one applies.</param>
/// <param name="ExecutionStatus">The execution status, for example <c>AVAILABLE</c>.</param>
/// <param name="Description">The change-set description, where one applies.</param>
/// <param name="CreationTime">The moment the change set was created.</param>
/// <param name="Parameters">The input parameters the change set carries.</param>
/// <param name="Capabilities">The capabilities the change set acknowledges.</param>
/// <param name="Changes">The resource changes the change set proposes.</param>
public sealed record CloudFormationChangeSetDetailResponse(
    string ChangeSetName,
    string ChangeSetId,
    string StackName,
    string StackId,
    string Status,
    string? StatusReason,
    string ExecutionStatus,
    string? Description,
    DateTimeOffset CreationTime,
    IReadOnlyList<StackParameterResponse> Parameters,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<CloudFormationResourceChangeResponse> Changes);

/// <summary>
/// A single resource change proposed by a CloudFormation change set.
/// </summary>
/// <param name="Action">The action CloudFormation will take, for example <c>Add</c>, <c>Modify</c>, or <c>Remove</c>.</param>
/// <param name="LogicalResourceId">The logical id the template assigns to the resource.</param>
/// <param name="PhysicalResourceId">The physical id of the existing resource, where one exists.</param>
/// <param name="ResourceType">The CloudFormation resource type, for example <c>AWS::SQS::Queue</c>.</param>
/// <param name="Replacement">Whether the change requires replacement, where applicable, for example <c>True</c>, <c>False</c>, or <c>Conditional</c>.</param>
public sealed record CloudFormationResourceChangeResponse(
    string Action,
    string LogicalResourceId,
    string? PhysicalResourceId,
    string ResourceType,
    string? Replacement);

/// <summary>
/// The details required to create a CloudFormation change set.
/// </summary>
/// <param name="StackName">The name of the stack the change set applies to.</param>
/// <param name="ChangeSetName">The name to assign to the change set.</param>
/// <param name="ChangeSetType">The change-set type, either <c>CREATE</c> or <c>UPDATE</c>.</param>
/// <param name="TemplateBody">The template body that defines the desired stack state.</param>
/// <param name="Parameters">The input parameters to deploy the change set with.</param>
/// <param name="Capabilities">The capabilities the template requires, such as <c>CAPABILITY_IAM</c>.</param>
public sealed record CloudFormationChangeSetCreateRequest(
    string StackName,
    string ChangeSetName,
    string ChangeSetType,
    string TemplateBody,
    IReadOnlyList<StackParameterRequest>? Parameters,
    IReadOnlyList<string>? Capabilities);

/// <summary>
/// The result of creating a CloudFormation change set.
/// </summary>
/// <param name="ChangeSetId">The Amazon Resource Name that uniquely identifies the change set.</param>
public sealed record CloudFormationChangeSetOperationResponse(
    string ChangeSetId);

/// <summary>
/// The result of starting CloudFormation drift detection for a stack.
/// </summary>
/// <param name="StackDriftDetectionId">The id used to poll the drift-detection operation.</param>
public sealed record CloudFormationDriftDetectionResponse(
    string StackDriftDetectionId);

/// <summary>
/// The status of a CloudFormation drift-detection operation.
/// </summary>
/// <param name="StackDriftDetectionId">The id of the drift-detection operation.</param>
/// <param name="StackId">The Amazon Resource Name that uniquely identifies the stack.</param>
/// <param name="DetectionStatus">The detection status, for example <c>DETECTION_COMPLETE</c> or <c>DETECTION_IN_PROGRESS</c>.</param>
/// <param name="DetectionStatusReason">The reason for the current detection status, where one applies.</param>
/// <param name="StackDriftStatus">The overall stack drift status, for example <c>DRIFTED</c> or <c>IN_SYNC</c>.</param>
/// <param name="DriftedStackResourceCount">The number of resources that have drifted.</param>
/// <param name="Timestamp">The moment the drift-detection status was reported.</param>
public sealed record CloudFormationDriftStatusResponse(
    string StackDriftDetectionId,
    string StackId,
    string DetectionStatus,
    string? DetectionStatusReason,
    string StackDriftStatus,
    int DriftedStackResourceCount,
    DateTimeOffset Timestamp);

/// <summary>
/// The per-resource drift results for a CloudFormation stack.
/// </summary>
/// <param name="Drifts">The resource drift records, ordered as returned by the backend.</param>
public sealed record CloudFormationResourceDriftListResponse(
    IReadOnlyList<CloudFormationResourceDriftResponse> Drifts);

/// <summary>
/// The drift result for a single CloudFormation stack resource.
/// </summary>
/// <param name="LogicalResourceId">The logical id the template assigns to the resource.</param>
/// <param name="PhysicalResourceId">The physical id of the resource, where one exists.</param>
/// <param name="ResourceType">The CloudFormation resource type, for example <c>AWS::SQS::Queue</c>.</param>
/// <param name="DriftStatus">The resource drift status, for example <c>MODIFIED</c>, <c>DELETED</c>, or <c>IN_SYNC</c>.</param>
/// <param name="ExpectedProperties">The properties expected from the template, where available.</param>
/// <param name="ActualProperties">The properties observed on the actual resource, where available.</param>
/// <param name="Timestamp">The moment the resource drift was reported.</param>
public sealed record CloudFormationResourceDriftResponse(
    string LogicalResourceId,
    string? PhysicalResourceId,
    string ResourceType,
    string DriftStatus,
    string? ExpectedProperties,
    string? ActualProperties,
    DateTimeOffset Timestamp);

/// <summary>
/// The exported output values published across all CloudFormation stacks for cross-stack references.
/// </summary>
/// <param name="Exports">The exports, ordered as returned by the backend.</param>
public sealed record CloudFormationExportListResponse(
    IReadOnlyList<CloudFormationExportResponse> Exports);

/// <summary>
/// A single value a CloudFormation stack publishes for other stacks to import.
/// </summary>
/// <param name="Name">The export name, unique within the account and region.</param>
/// <param name="Value">The exported value.</param>
/// <param name="ExportingStackId">The Amazon Resource Name of the stack that publishes the export.</param>
public sealed record CloudFormationExportResponse(
    string Name,
    string Value,
    string ExportingStackId);

/// <summary>
/// The names of the CloudFormation stacks that import a single exported value.
/// </summary>
/// <param name="ImportingStackNames">The names of the importing stacks.</param>
public sealed record CloudFormationImportListResponse(
    IReadOnlyList<string> ImportingStackNames);

