using System.Diagnostics.CodeAnalysis;
using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudFormation;
using Foundation.Domain.CloudFormation;
using Foundation.Infrastructure.Aws;

namespace Foundation.Infrastructure.CloudFormation;

/// <summary>
/// Reads CloudFormation through the resilient AWS gateway so the same code works against LocalStack
/// or real AWS. All access flows through <see cref="IAwsGateway"/>, which records capability and
/// converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class CloudFormationClientAdapter : ICloudFormationClient
{
    private const string ServiceKey = "cloudformation";

    private readonly IAwsGateway _gateway;

    public CloudFormationClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<CloudFormationStackSummary>>> ListStacksAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, IReadOnlyList<CloudFormationStackSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var stacks = new List<CloudFormationStackSummary>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListStacksAsync(
                        new ListStacksRequest { NextToken = nextToken },
                        token);

                    foreach (var stack in response.StackSummaries ?? [])
                    {
                        if (stack.StackStatus == StackStatus.DELETE_COMPLETE)
                            continue;

                        stacks.Add(ToSummary(stack));
                    }

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return stacks;
            },
            cancellationToken);

    public Task<Result<CloudFormationStackDetail>> DescribeStackAsync(
        string stackName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, CloudFormationStackDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.DescribeStacksAsync(
                    new DescribeStacksRequest { StackName = stackName },
                    token);

                var stack = response.Stacks![0];

                return ToDetail(stack);
            },
            cancellationToken);

    public Task<Result<CloudFormationStackTemplate>> GetTemplateAsync(
        string stackName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, CloudFormationStackTemplate>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetTemplateAsync(
                    new GetTemplateRequest { StackName = stackName },
                    token);

                var body = response.TemplateBody ?? string.Empty;

                return new CloudFormationStackTemplate(body, DetectFormat(body));
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<Foundation.Domain.CloudFormation.StackResource>>> ListStackResourcesAsync(
        string stackName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, IReadOnlyList<Foundation.Domain.CloudFormation.StackResource>>(
            ServiceKey,
            async (client, token) =>
            {
                var resources = new List<Foundation.Domain.CloudFormation.StackResource>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListStackResourcesAsync(
                        new ListStackResourcesRequest { StackName = stackName, NextToken = nextToken },
                        token);

                    foreach (var resource in response.StackResourceSummaries ?? [])
                        resources.Add(ToResource(resource));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return resources;
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<Foundation.Domain.CloudFormation.StackEvent>>> DescribeStackEventsAsync(
        string stackName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, IReadOnlyList<Foundation.Domain.CloudFormation.StackEvent>>(
            ServiceKey,
            async (client, token) =>
            {
                var events = new List<Foundation.Domain.CloudFormation.StackEvent>();
                string? nextToken = null;

                do
                {
                    var response = await client.DescribeStackEventsAsync(
                        new DescribeStackEventsRequest { StackName = stackName, NextToken = nextToken },
                        token);

                    foreach (var stackEvent in response.StackEvents ?? [])
                        events.Add(ToEvent(stackEvent));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return events;
            },
            cancellationToken);

    public Task<Result<string>> CreateStackAsync(
        string stackName,
        string templateBody,
        IReadOnlyList<Foundation.Domain.CloudFormation.StackParameter> parameters,
        IReadOnlyList<string> capabilities,
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, string>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.CreateStackAsync(
                    new CreateStackRequest
                    {
                        StackName = stackName,
                        TemplateBody = templateBody,
                        Parameters = ToParameters(parameters),
                        Capabilities = capabilities.ToList(),
                    },
                    token);

                return response.StackId ?? string.Empty;
            },
            cancellationToken);

    public Task<Result<string>> UpdateStackAsync(
        string stackName,
        string templateBody,
        IReadOnlyList<Foundation.Domain.CloudFormation.StackParameter> parameters,
        IReadOnlyList<string> capabilities,
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, string>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.UpdateStackAsync(
                    new UpdateStackRequest
                    {
                        StackName = stackName,
                        TemplateBody = templateBody,
                        Parameters = ToParameters(parameters),
                        Capabilities = capabilities.ToList(),
                    },
                    token);

                return response.StackId ?? string.Empty;
            },
            cancellationToken);

    public async Task<Result> DeleteStackAsync(string stackName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonCloudFormationClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteStackAsync(
                    new DeleteStackRequest { StackName = stackName }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<string>> CreateChangeSetAsync(
        string stackName,
        string changeSetName,
        string changeSetType,
        string templateBody,
        IReadOnlyList<Foundation.Domain.CloudFormation.StackParameter> parameters,
        IReadOnlyList<string> capabilities,
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, string>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.CreateChangeSetAsync(
                    new CreateChangeSetRequest
                    {
                        StackName = stackName,
                        ChangeSetName = changeSetName,
                        ChangeSetType = changeSetType,
                        TemplateBody = templateBody,
                        Parameters = ToParameters(parameters),
                        Capabilities = capabilities.ToList(),
                    },
                    token);

                return response.Id ?? string.Empty;
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<Foundation.Domain.CloudFormation.ChangeSetSummary>>> ListChangeSetsAsync(
        string stackName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, IReadOnlyList<Foundation.Domain.CloudFormation.ChangeSetSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var changeSets = new List<Foundation.Domain.CloudFormation.ChangeSetSummary>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListChangeSetsAsync(
                        new ListChangeSetsRequest { StackName = stackName, NextToken = nextToken },
                        token);

                    foreach (var summary in response.Summaries ?? [])
                        changeSets.Add(ToChangeSetSummary(summary));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return changeSets;
            },
            cancellationToken);

    public Task<Result<ChangeSetDetail>> DescribeChangeSetAsync(
        string stackName, string changeSetName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, ChangeSetDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var changes = new List<Foundation.Domain.CloudFormation.ResourceChange>();
                DescribeChangeSetResponse response;
                string? nextToken = null;

                do
                {
                    response = await client.DescribeChangeSetAsync(
                        new DescribeChangeSetRequest
                        {
                            StackName = stackName,
                            ChangeSetName = changeSetName,
                            NextToken = nextToken,
                        },
                        token);

                    foreach (var change in response.Changes ?? [])
                    {
                        if (change.ResourceChange is { } resourceChange)
                            changes.Add(ToResourceChange(resourceChange));
                    }

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return ToChangeSetDetail(response, changes);
            },
            cancellationToken);

    public async Task<Result> ExecuteChangeSetAsync(
        string stackName, string changeSetName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonCloudFormationClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.ExecuteChangeSetAsync(
                    new ExecuteChangeSetRequest { StackName = stackName, ChangeSetName = changeSetName },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteChangeSetAsync(
        string stackName, string changeSetName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonCloudFormationClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteChangeSetAsync(
                    new DeleteChangeSetRequest { StackName = stackName, ChangeSetName = changeSetName },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<string>> DetectStackDriftAsync(
        string stackName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, string>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.DetectStackDriftAsync(
                    new DetectStackDriftRequest { StackName = stackName },
                    token);

                return response.StackDriftDetectionId ?? string.Empty;
            },
            cancellationToken);

    public Task<Result<Foundation.Domain.CloudFormation.StackDriftStatus>> DescribeStackDriftDetectionStatusAsync(
        string driftDetectionId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, Foundation.Domain.CloudFormation.StackDriftStatus>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.DescribeStackDriftDetectionStatusAsync(
                    new DescribeStackDriftDetectionStatusRequest { StackDriftDetectionId = driftDetectionId },
                    token);

                return ToDriftStatus(response);
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<Foundation.Domain.CloudFormation.StackResourceDrift>>> DescribeStackResourceDriftsAsync(
        string stackName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, IReadOnlyList<Foundation.Domain.CloudFormation.StackResourceDrift>>(
            ServiceKey,
            async (client, token) =>
            {
                var drifts = new List<Foundation.Domain.CloudFormation.StackResourceDrift>();
                string? nextToken = null;

                do
                {
                    var response = await client.DescribeStackResourceDriftsAsync(
                        new DescribeStackResourceDriftsRequest { StackName = stackName, NextToken = nextToken },
                        token);

                    foreach (var drift in response.StackResourceDrifts ?? [])
                        drifts.Add(ToResourceDrift(drift));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return drifts;
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<Foundation.Domain.CloudFormation.StackExport>>> ListExportsAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, IReadOnlyList<Foundation.Domain.CloudFormation.StackExport>>(
            ServiceKey,
            async (client, token) =>
            {
                var exports = new List<Foundation.Domain.CloudFormation.StackExport>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListExportsAsync(
                        new ListExportsRequest { NextToken = nextToken },
                        token);

                    foreach (var export in response.Exports ?? [])
                        exports.Add(ToExport(export));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return exports;
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<string>>> ListImportsAsync(
        string exportName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudFormationClient, IReadOnlyList<string>>(
            ServiceKey,
            async (client, token) =>
            {
                var imports = new List<string>();
                string? nextToken = null;

                do
                {
                    ListImportsResponse response;
                    try
                    {
                        response = await client.ListImportsAsync(
                            new ListImportsRequest { ExportName = exportName, NextToken = nextToken },
                            token);
                    }
                    catch (AmazonCloudFormationException)
                    {
                        // The backend reports an export that no stack imports as a validation error;
                        // treat that as an empty result rather than a failure.
                        break;
                    }

                    foreach (var import in response.Imports ?? [])
                        imports.Add(import);

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return imports;
            },
            cancellationToken);

    private static Foundation.Domain.CloudFormation.StackExport ToExport(Export export)
        => new(
            export.Name ?? string.Empty,
            export.Value ?? string.Empty,
            export.ExportingStackId ?? string.Empty);

    private static List<Parameter> ToParameters(
        IReadOnlyList<Foundation.Domain.CloudFormation.StackParameter> parameters)
        => parameters
            .Select(parameter => new Parameter
            {
                ParameterKey = parameter.ParameterKey,
                ParameterValue = parameter.ParameterValue,
            })
            .ToList();

    private static string DetectFormat(string templateBody)
        => templateBody.TrimStart().StartsWith('{') ? "json" : "yaml";

    private static Foundation.Domain.CloudFormation.ChangeSetSummary ToChangeSetSummary(
        Amazon.CloudFormation.Model.ChangeSetSummary summary)
        => new(
            summary.ChangeSetId ?? string.Empty,
            summary.ChangeSetName ?? string.Empty,
            summary.StackName ?? string.Empty,
            summary.Status?.Value ?? string.Empty,
            summary.StatusReason,
            summary.ExecutionStatus?.Value ?? string.Empty,
            summary.Description,
            summary.CreationTime ?? default);

    private static ChangeSetDetail ToChangeSetDetail(
        DescribeChangeSetResponse response, IReadOnlyList<Foundation.Domain.CloudFormation.ResourceChange> changes)
        => new(
            response.ChangeSetName ?? string.Empty,
            response.ChangeSetId ?? string.Empty,
            response.StackName ?? string.Empty,
            response.StackId ?? string.Empty,
            response.Status?.Value ?? string.Empty,
            response.StatusReason,
            response.ExecutionStatus?.Value ?? string.Empty,
            response.Description,
            response.CreationTime ?? default,
            (response.Parameters ?? [])
                .Select(parameter => new StackParameter(
                    parameter.ParameterKey ?? string.Empty,
                    parameter.ParameterValue ?? string.Empty))
                .ToList(),
            response.Capabilities ?? [],
            changes);

    private static Foundation.Domain.CloudFormation.ResourceChange ToResourceChange(
        Amazon.CloudFormation.Model.ResourceChange change)
        => new(
            change.Action?.Value ?? string.Empty,
            change.LogicalResourceId ?? string.Empty,
            change.PhysicalResourceId,
            change.ResourceType ?? string.Empty,
            change.Replacement?.Value);

    private static Foundation.Domain.CloudFormation.StackDriftStatus ToDriftStatus(
        DescribeStackDriftDetectionStatusResponse response)
        => new(
            response.StackDriftDetectionId ?? string.Empty,
            response.StackId ?? string.Empty,
            response.DetectionStatus?.Value ?? string.Empty,
            response.DetectionStatusReason,
            response.StackDriftStatus?.Value ?? string.Empty,
            response.DriftedStackResourceCount ?? 0,
            response.Timestamp ?? default);

    private static Foundation.Domain.CloudFormation.StackResourceDrift ToResourceDrift(
        Amazon.CloudFormation.Model.StackResourceDrift drift)
        => new(
            drift.LogicalResourceId ?? string.Empty,
            drift.PhysicalResourceId,
            drift.ResourceType ?? string.Empty,
            drift.StackResourceDriftStatus?.Value ?? string.Empty,
            drift.ExpectedProperties,
            drift.ActualProperties,
            drift.Timestamp ?? default);

    private static Foundation.Domain.CloudFormation.StackEvent ToEvent(
        Amazon.CloudFormation.Model.StackEvent stackEvent)
        => new(
            stackEvent.EventId ?? string.Empty,
            stackEvent.Timestamp ?? default,
            stackEvent.LogicalResourceId ?? string.Empty,
            stackEvent.PhysicalResourceId,
            stackEvent.ResourceType ?? string.Empty,
            stackEvent.ResourceStatus?.Value ?? string.Empty,
            stackEvent.ResourceStatusReason);

    private static Foundation.Domain.CloudFormation.StackResource ToResource(StackResourceSummary resource)
        => new(
            resource.LogicalResourceId ?? string.Empty,
            resource.PhysicalResourceId,
            resource.ResourceType ?? string.Empty,
            resource.ResourceStatus?.Value ?? string.Empty,
            resource.ResourceStatusReason,
            resource.LastUpdatedTimestamp ?? default);

    private static CloudFormationStackSummary ToSummary(StackSummary stack)
        => new(
            stack.StackName ?? string.Empty,
            stack.StackId ?? string.Empty,
            stack.StackStatus?.Value ?? string.Empty,
            stack.TemplateDescription,
            stack.CreationTime ?? default,
            stack.LastUpdatedTime);

    private static CloudFormationStackDetail ToDetail(Stack stack)
        => new(
            stack.StackName ?? string.Empty,
            stack.StackId ?? string.Empty,
            stack.StackStatus?.Value ?? string.Empty,
            stack.StackStatusReason,
            stack.Description,
            stack.CreationTime ?? default,
            stack.LastUpdatedTime,
            (stack.Parameters ?? [])
                .Select(parameter => new StackParameter(
                    parameter.ParameterKey ?? string.Empty,
                    parameter.ParameterValue ?? string.Empty))
                .ToList(),
            (stack.Outputs ?? [])
                .Select(output => new StackOutput(
                    output.OutputKey ?? string.Empty,
                    output.OutputValue ?? string.Empty,
                    output.Description,
                    output.ExportName))
                .ToList(),
            (stack.Tags ?? [])
                .Select(tag => new StackTag(
                    tag.Key ?? string.Empty,
                    tag.Value ?? string.Empty))
                .ToList(),
            stack.Capabilities ?? []);
}
