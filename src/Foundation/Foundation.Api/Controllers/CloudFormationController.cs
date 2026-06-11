using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateChangeSet;
using Foundation.Application.Commands.CreateStack;
using Foundation.Application.Commands.DeleteChangeSet;
using Foundation.Application.Commands.DeleteStack;
using Foundation.Application.Commands.DetectStackDrift;
using Foundation.Application.Commands.ExecuteChangeSet;
using Foundation.Application.Commands.UpdateStack;
using Foundation.Application.Queries.DescribeChangeSet;
using Foundation.Application.Queries.GetDriftStatus;
using Foundation.Application.Queries.GetStack;
using Foundation.Application.Queries.GetStackTemplate;
using Foundation.Application.Queries.ListChangeSets;
using Foundation.Application.Queries.ListExports;
using Foundation.Application.Queries.ListImports;
using Foundation.Application.Queries.ListResourceDrifts;
using Foundation.Application.Queries.ListStackEvents;
using Foundation.Application.Queries.ListStackResources;
using Foundation.Application.Queries.ListStacks;
using Foundation.Application.Queries.ValidateTemplate;
using Foundation.Domain.CloudFormation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to AWS CloudFormation: listing the available stacks and viewing the details of a
/// single stack.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/cloudformation")]
public partial class CloudFormationController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudFormationController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public CloudFormationController(ISender sender, ILogger<CloudFormationController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the CloudFormation stacks available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the stack summaries.</returns>
    [HttpGet("stacks")]
    [ProducesResponseType(typeof(CloudFormationStackListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListStacks(CancellationToken cancellationToken)
    {
        LogHandlingListStacks();
        var result = await _sender.Send(new ListStacksQuery(), cancellationToken);
        LogListStacksHandled(result.IsSuccess);
        return result.Match(
            stacks => Results.Ok(new CloudFormationStackListResponse(
                stacks.Stacks
                    .Select(stack => new CloudFormationStackSummaryResponse(
                        stack.StackName,
                        stack.StackId,
                        stack.StackStatus,
                        stack.Description,
                        stack.CreationTime,
                        stack.LastUpdatedTime))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the details of a single CloudFormation stack by its name.
    /// </summary>
    /// <param name="name">The name of the stack to describe.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the stack details.</returns>
    [HttpGet("stack")]
    [ProducesResponseType(typeof(CloudFormationStackDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetStack(
        [FromQuery] string name, CancellationToken cancellationToken)
    {
        LogHandlingGetStack(name);
        var result = await _sender.Send(new GetStackQuery(name), cancellationToken);
        LogGetStackHandled(result.IsSuccess);
        return result.Match(
            stack => Results.Ok(new CloudFormationStackDetailResponse(
                stack.Stack.StackName,
                stack.Stack.StackId,
                stack.Stack.StackStatus,
                stack.Stack.StackStatusReason,
                stack.Stack.Description,
                stack.Stack.CreationTime,
                stack.Stack.LastUpdatedTime,
                stack.Stack.Parameters
                    .Select(parameter => new StackParameterResponse(
                        parameter.ParameterKey,
                        parameter.ParameterValue))
                    .ToList(),
                stack.Stack.Outputs
                    .Select(output => new StackOutputResponse(
                        output.OutputKey,
                        output.OutputValue,
                        output.Description,
                        output.ExportName))
                    .ToList(),
                stack.Stack.Tags
                    .Select(tag => new StackTagResponse(
                        tag.Key,
                        tag.Value))
                    .ToList(),
                stack.Stack.Capabilities)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the template body that defines a single CloudFormation stack by its name.
    /// </summary>
    /// <param name="name">The name of the stack whose template to get.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the stack template.</returns>
    [HttpGet("stack/template")]
    [ProducesResponseType(typeof(CloudFormationStackTemplateResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetStackTemplate(
        [FromQuery] string name, CancellationToken cancellationToken)
    {
        LogHandlingGetStackTemplate(name);
        var result = await _sender.Send(new GetStackTemplateQuery(name), cancellationToken);
        LogGetStackTemplateHandled(result.IsSuccess);
        return result.Match(
            template => Results.Ok(new CloudFormationStackTemplateResponse(
                template.Template.TemplateBody,
                template.Template.Format)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the resources a single CloudFormation stack manages by its name.
    /// </summary>
    /// <param name="name">The name of the stack whose resources to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the stack resources.</returns>
    [HttpGet("stack/resources")]
    [ProducesResponseType(typeof(CloudFormationStackResourceListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListStackResources(
        [FromQuery] string name, CancellationToken cancellationToken)
    {
        LogHandlingListStackResources(name);
        var result = await _sender.Send(new ListStackResourcesQuery(name), cancellationToken);
        LogListStackResourcesHandled(result.IsSuccess);
        return result.Match(
            resources => Results.Ok(new CloudFormationStackResourceListResponse(
                resources.Resources
                    .Select(resource => new CloudFormationStackResourceResponse(
                        resource.LogicalResourceId,
                        resource.PhysicalResourceId,
                        resource.ResourceType,
                        resource.ResourceStatus,
                        resource.ResourceStatusReason,
                        resource.LastUpdatedTime))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the events recorded for a single CloudFormation stack by its name.
    /// </summary>
    /// <param name="name">The name of the stack whose events to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the stack events.</returns>
    [HttpGet("stack/events")]
    [ProducesResponseType(typeof(CloudFormationStackEventListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListStackEvents(
        [FromQuery] string name, CancellationToken cancellationToken)
    {
        LogHandlingListStackEvents(name);
        var result = await _sender.Send(new ListStackEventsQuery(name), cancellationToken);
        LogListStackEventsHandled(result.IsSuccess);
        return result.Match(
            events => Results.Ok(new CloudFormationStackEventListResponse(
                events.Events
                    .Select(stackEvent => new CloudFormationStackEventResponse(
                        stackEvent.EventId,
                        stackEvent.Timestamp,
                        stackEvent.LogicalResourceId,
                        stackEvent.PhysicalResourceId,
                        stackEvent.ResourceType,
                        stackEvent.ResourceStatus,
                        stackEvent.ResourceStatusReason))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new CloudFormation stack from the supplied template, parameters, and capabilities.
    /// </summary>
    /// <param name="request">The stack to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new stack id.</returns>
    [HttpPost("stack")]
    [ProducesResponseType(typeof(CloudFormationStackOperationResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateStack(
        [FromBody] CloudFormationStackCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateStack(request.StackName);
        var result = await _sender.Send(
            new CreateStackCommand(
                request.StackName,
                request.TemplateBody,
                request.TemplateUrl,
                request.Parameters
                    ?.Select(parameter => new StackParameter(parameter.ParameterKey, parameter.ParameterValue))
                    .ToList() ?? [],
                request.Capabilities ?? []),
            cancellationToken);
        LogCreateStackHandled(result.IsSuccess);
        return result.Match(
            stackId => Results.Created(
                $"/api/services/cloudformation/stack?name={Uri.EscapeDataString(request.StackName)}",
                new CloudFormationStackOperationResponse(stackId)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Validates a CloudFormation template supplied either inline or by S3 URL.
    /// </summary>
    /// <param name="request">The template body or S3 URL to validate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result describing the validated template.</returns>
    [HttpPost("template/validate")]
    [ProducesResponseType(typeof(CloudFormationTemplateValidationResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ValidateTemplate(
        [FromBody] CloudFormationTemplateValidationRequest request, CancellationToken cancellationToken)
    {
        LogHandlingValidateTemplate();
        var result = await _sender.Send(
            new ValidateTemplateQuery(request.TemplateBody, request.TemplateUrl),
            cancellationToken);
        LogValidateTemplateHandled(result.IsSuccess);
        return result.Match(
            value => Results.Ok(new CloudFormationTemplateValidationResponse(
                value.Validation.Description,
                value.Validation.CapabilitiesReason,
                value.Validation.Capabilities,
                value.Validation.Parameters
                    .Select(parameter => new CloudFormationTemplateValidationParameterResponse(
                        parameter.ParameterKey,
                        parameter.DefaultValue,
                        parameter.NoEcho,
                        parameter.Description))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates an existing CloudFormation stack with the supplied template, parameters, and capabilities.
    /// </summary>
    /// <param name="name">The name of the stack to update.</param>
    /// <param name="request">The new template, parameters, and capabilities for the stack.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the updated stack id.</returns>
    [HttpPut("stack")]
    [ProducesResponseType(typeof(CloudFormationStackOperationResponse), StatusCodes.Status200OK)]
    public async Task<IResult> UpdateStack(
        [FromQuery] string name,
        [FromBody] CloudFormationStackUpdateRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingUpdateStack(name);
        var result = await _sender.Send(
            new UpdateStackCommand(
                name,
                request.TemplateBody,
                request.Parameters
                    ?.Select(parameter => new StackParameter(parameter.ParameterKey, parameter.ParameterValue))
                    .ToList() ?? [],
                request.Capabilities ?? []),
            cancellationToken);
        LogUpdateStackHandled(result.IsSuccess);
        return result.Match(
            stackId => Results.Ok(new CloudFormationStackOperationResponse(stackId)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a CloudFormation stack and all of the resources it manages. This is a destructive
    /// action that cannot be undone.
    /// </summary>
    /// <param name="name">The name of the stack to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("stack")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteStack(
        [FromQuery] string name, CancellationToken cancellationToken)
    {
        LogHandlingDeleteStack(name);
        var result = await _sender.Send(new DeleteStackCommand(name), cancellationToken);
        LogDeleteStackHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the change sets associated with a single CloudFormation stack by its name.
    /// </summary>
    /// <param name="name">The name of the stack whose change sets to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the change-set summaries.</returns>
    [HttpGet("change-sets")]
    [ProducesResponseType(typeof(CloudFormationChangeSetListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListChangeSets(
        [FromQuery] string name, CancellationToken cancellationToken)
    {
        LogHandlingListChangeSets(name);
        var result = await _sender.Send(new ListChangeSetsQuery(name), cancellationToken);
        LogListChangeSetsHandled(result.IsSuccess);
        return result.Match(
            changeSets => Results.Ok(new CloudFormationChangeSetListResponse(
                changeSets.ChangeSets
                    .Select(changeSet => new CloudFormationChangeSetSummaryResponse(
                        changeSet.ChangeSetId,
                        changeSet.ChangeSetName,
                        changeSet.StackName,
                        changeSet.Status,
                        changeSet.StatusReason,
                        changeSet.ExecutionStatus,
                        changeSet.Description,
                        changeSet.CreationTime))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the details of a single CloudFormation change set, including the resource changes it proposes.
    /// </summary>
    /// <param name="name">The name of the stack the change set applies to.</param>
    /// <param name="changeSet">The name of the change set to describe.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the change-set details.</returns>
    [HttpGet("change-set")]
    [ProducesResponseType(typeof(CloudFormationChangeSetDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> DescribeChangeSet(
        [FromQuery] string name,
        [FromQuery] string changeSet,
        CancellationToken cancellationToken)
    {
        LogHandlingDescribeChangeSet(changeSet, name);
        var result = await _sender.Send(
            new DescribeChangeSetQuery(name, changeSet), cancellationToken);
        LogDescribeChangeSetHandled(result.IsSuccess);
        return result.Match(
            detail => Results.Ok(new CloudFormationChangeSetDetailResponse(
                detail.ChangeSet.ChangeSetName,
                detail.ChangeSet.ChangeSetId,
                detail.ChangeSet.StackName,
                detail.ChangeSet.StackId,
                detail.ChangeSet.Status,
                detail.ChangeSet.StatusReason,
                detail.ChangeSet.ExecutionStatus,
                detail.ChangeSet.Description,
                detail.ChangeSet.CreationTime,
                detail.ChangeSet.Parameters
                    .Select(parameter => new StackParameterResponse(
                        parameter.ParameterKey,
                        parameter.ParameterValue))
                    .ToList(),
                detail.ChangeSet.Capabilities,
                detail.ChangeSet.Changes
                    .Select(change => new CloudFormationResourceChangeResponse(
                        change.Action,
                        change.LogicalResourceId,
                        change.PhysicalResourceId,
                        change.ResourceType,
                        change.Replacement))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a CloudFormation change set that previews the changes a template would apply to a stack.
    /// </summary>
    /// <param name="request">The change set to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new change-set id.</returns>
    [HttpPost("change-set")]
    [ProducesResponseType(typeof(CloudFormationChangeSetOperationResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateChangeSet(
        [FromBody] CloudFormationChangeSetCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateChangeSet(request.ChangeSetName, request.StackName);
        var result = await _sender.Send(
            new CreateChangeSetCommand(
                request.StackName,
                request.ChangeSetName,
                request.ChangeSetType,
                request.TemplateBody,
                request.Parameters
                    ?.Select(parameter => new StackParameter(parameter.ParameterKey, parameter.ParameterValue))
                    .ToList() ?? [],
                request.Capabilities ?? []),
            cancellationToken);
        LogCreateChangeSetHandled(result.IsSuccess);
        return result.Match(
            changeSetId => Results.Created(
                $"/api/services/cloudformation/change-set?name={Uri.EscapeDataString(request.StackName)}&changeSet={Uri.EscapeDataString(request.ChangeSetName)}",
                new CloudFormationChangeSetOperationResponse(changeSetId)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Executes a CloudFormation change set, applying its proposed changes to the stack.
    /// </summary>
    /// <param name="name">The name of the stack the change set applies to.</param>
    /// <param name="changeSet">The name of the change set to execute.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPost("change-set/execute")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> ExecuteChangeSet(
        [FromQuery] string name,
        [FromQuery] string changeSet,
        CancellationToken cancellationToken)
    {
        LogHandlingExecuteChangeSet(changeSet, name);
        var result = await _sender.Send(
            new ExecuteChangeSetCommand(name, changeSet), cancellationToken);
        LogExecuteChangeSetHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a CloudFormation change set without applying its proposed changes.
    /// </summary>
    /// <param name="name">The name of the stack the change set applies to.</param>
    /// <param name="changeSet">The name of the change set to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("change-set")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteChangeSet(
        [FromQuery] string name,
        [FromQuery] string changeSet,
        CancellationToken cancellationToken)
    {
        LogHandlingDeleteChangeSet(changeSet, name);
        var result = await _sender.Send(
            new DeleteChangeSetCommand(name, changeSet), cancellationToken);
        LogDeleteChangeSetHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Starts CloudFormation drift detection for a stack, comparing its actual resources against its template.
    /// </summary>
    /// <param name="name">The name of the stack to detect drift for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 202 result carrying the drift-detection id used to poll for the result.</returns>
    [HttpPost("stack/drift")]
    [ProducesResponseType(typeof(CloudFormationDriftDetectionResponse), StatusCodes.Status202Accepted)]
    public async Task<IResult> DetectStackDrift(
        [FromQuery] string name, CancellationToken cancellationToken)
    {
        LogHandlingDetectStackDrift(name);
        var result = await _sender.Send(new DetectStackDriftCommand(name), cancellationToken);
        LogDetectStackDriftHandled(result.IsSuccess);
        return result.Match(
            driftDetectionId => Results.Accepted(
                $"/api/services/cloudformation/stack/drift?driftDetectionId={Uri.EscapeDataString(driftDetectionId)}",
                new CloudFormationDriftDetectionResponse(driftDetectionId)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the status of a CloudFormation drift-detection operation.
    /// </summary>
    /// <param name="driftDetectionId">The id of the drift-detection operation to poll.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the drift-detection status.</returns>
    [HttpGet("stack/drift")]
    [ProducesResponseType(typeof(CloudFormationDriftStatusResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetDriftStatus(
        [FromQuery] string driftDetectionId, CancellationToken cancellationToken)
    {
        LogHandlingGetDriftStatus(driftDetectionId);
        var result = await _sender.Send(new GetDriftStatusQuery(driftDetectionId), cancellationToken);
        LogGetDriftStatusHandled(result.IsSuccess);
        return result.Match(
            status => Results.Ok(new CloudFormationDriftStatusResponse(
                status.Status.StackDriftDetectionId,
                status.Status.StackId,
                status.Status.DetectionStatus,
                status.Status.DetectionStatusReason,
                status.Status.StackDriftStatusValue,
                status.Status.DriftedStackResourceCount,
                status.Status.Timestamp)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the per-resource drift results for a CloudFormation stack.
    /// </summary>
    /// <param name="name">The name of the stack to list resource drift for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the per-resource drift records.</returns>
    [HttpGet("stack/drift/resources")]
    [ProducesResponseType(typeof(CloudFormationResourceDriftListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListResourceDrifts(
        [FromQuery] string name, CancellationToken cancellationToken)
    {
        LogHandlingListResourceDrifts(name);
        var result = await _sender.Send(new ListResourceDriftsQuery(name), cancellationToken);
        LogListResourceDriftsHandled(result.IsSuccess);
        return result.Match(
            drifts => Results.Ok(new CloudFormationResourceDriftListResponse(
                drifts.Drifts
                    .Select(drift => new CloudFormationResourceDriftResponse(
                        drift.LogicalResourceId,
                        drift.PhysicalResourceId,
                        drift.ResourceType,
                        drift.DriftStatus,
                        drift.ExpectedProperties,
                        drift.ActualProperties,
                        drift.Timestamp))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the exported output values published across all CloudFormation stacks for cross-stack references.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the exports.</returns>
    [HttpGet("exports")]
    [ProducesResponseType(typeof(CloudFormationExportListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListExports(CancellationToken cancellationToken)
    {
        LogHandlingListExports();
        var result = await _sender.Send(new ListExportsQuery(), cancellationToken);
        LogListExportsHandled(result.IsSuccess);
        return result.Match(
            exports => Results.Ok(new CloudFormationExportListResponse(
                exports.Exports
                    .Select(export => new CloudFormationExportResponse(
                        export.Name,
                        export.Value,
                        export.ExportingStackId))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the names of the CloudFormation stacks that import a single exported value.
    /// </summary>
    /// <param name="exportName">The export name whose importing stacks to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the importing stack names.</returns>
    [HttpGet("exports/{exportName}/imports")]
    [ProducesResponseType(typeof(CloudFormationImportListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListImports(
        string exportName, CancellationToken cancellationToken)
    {
        LogHandlingListImports(exportName);
        var result = await _sender.Send(new ListImportsQuery(exportName), cancellationToken);
        LogListImportsHandled(result.IsSuccess);
        return result.Match(
            imports => Results.Ok(new CloudFormationImportListResponse(
                imports.ImportingStackNames)),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation stack list request.")]
    private partial void LogHandlingListStacks();

    [LoggerMessage(LogLevel.Trace, "CloudFormation stack list request handled. Success: {Success}")]
    private partial void LogListStacksHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation stack get request for {Name}.")]
    private partial void LogHandlingGetStack(string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation stack get request handled. Success: {Success}")]
    private partial void LogGetStackHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation stack template request for {Name}.")]
    private partial void LogHandlingGetStackTemplate(string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation stack template request handled. Success: {Success}")]
    private partial void LogGetStackTemplateHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation stack resources request for {Name}.")]
    private partial void LogHandlingListStackResources(string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation stack resources request handled. Success: {Success}")]
    private partial void LogListStackResourcesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation stack events request for {Name}.")]
    private partial void LogHandlingListStackEvents(string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation stack events request handled. Success: {Success}")]
    private partial void LogListStackEventsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation stack create request for {Name}.")]
    private partial void LogHandlingCreateStack(string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation stack create request handled. Success: {Success}")]
    private partial void LogCreateStackHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation template validation request.")]
    private partial void LogHandlingValidateTemplate();

    [LoggerMessage(LogLevel.Trace, "CloudFormation template validation request handled. Success: {Success}")]
    private partial void LogValidateTemplateHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation stack update request for {Name}.")]
    private partial void LogHandlingUpdateStack(string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation stack update request handled. Success: {Success}")]
    private partial void LogUpdateStackHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation stack delete request for {Name}.")]
    private partial void LogHandlingDeleteStack(string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation stack delete request handled. Success: {Success}")]
    private partial void LogDeleteStackHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation change-set list request for {Name}.")]
    private partial void LogHandlingListChangeSets(string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation change-set list request handled. Success: {Success}")]
    private partial void LogListChangeSetsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation change-set describe request for {ChangeSet} on {Name}.")]
    private partial void LogHandlingDescribeChangeSet(string changeSet, string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation change-set describe request handled. Success: {Success}")]
    private partial void LogDescribeChangeSetHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation change-set create request for {ChangeSet} on {Name}.")]
    private partial void LogHandlingCreateChangeSet(string changeSet, string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation change-set create request handled. Success: {Success}")]
    private partial void LogCreateChangeSetHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation change-set execute request for {ChangeSet} on {Name}.")]
    private partial void LogHandlingExecuteChangeSet(string changeSet, string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation change-set execute request handled. Success: {Success}")]
    private partial void LogExecuteChangeSetHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation change-set delete request for {ChangeSet} on {Name}.")]
    private partial void LogHandlingDeleteChangeSet(string changeSet, string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation change-set delete request handled. Success: {Success}")]
    private partial void LogDeleteChangeSetHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation drift detect request for {Name}.")]
    private partial void LogHandlingDetectStackDrift(string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation drift detect request handled. Success: {Success}")]
    private partial void LogDetectStackDriftHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation drift status request for {DriftDetectionId}.")]
    private partial void LogHandlingGetDriftStatus(string driftDetectionId);

    [LoggerMessage(LogLevel.Trace, "CloudFormation drift status request handled. Success: {Success}")]
    private partial void LogGetDriftStatusHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation resource drift list request for {Name}.")]
    private partial void LogHandlingListResourceDrifts(string name);

    [LoggerMessage(LogLevel.Trace, "CloudFormation resource drift list request handled. Success: {Success}")]
    private partial void LogListResourceDriftsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation export list request.")]
    private partial void LogHandlingListExports();

    [LoggerMessage(LogLevel.Trace, "CloudFormation export list request handled. Success: {Success}")]
    private partial void LogListExportsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CloudFormation import list request for {ExportName}.")]
    private partial void LogHandlingListImports(string exportName);

    [LoggerMessage(LogLevel.Trace, "CloudFormation import list request handled. Success: {Success}")]
    private partial void LogListImportsHandled(bool success);
}
