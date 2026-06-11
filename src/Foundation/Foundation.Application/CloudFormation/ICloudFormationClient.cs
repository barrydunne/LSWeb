using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.CloudFormation;

/// <summary>
/// Abstracts the CloudFormation operations the application needs so the handlers stay free of any
/// direct AWS SDK dependency. The implementation flows every call through the resilient AWS gateway
/// and translates failures into a <see cref="Result"/> rather than throwing.
/// </summary>
public interface ICloudFormationClient
{
    /// <summary>
    /// List the stacks available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stacks, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<CloudFormationStackSummary>>> ListStacksAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Describe a single stack by its name or Amazon Resource Name.
    /// </summary>
    /// <param name="stackName">The name or Amazon Resource Name of the stack to describe.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stack details, or an error when the backend cannot be reached.</returns>
    Task<Result<CloudFormationStackDetail>> DescribeStackAsync(
        string stackName, CancellationToken cancellationToken);

    /// <summary>
    /// Get the template body that defines a single stack.
    /// </summary>
    /// <param name="stackName">The name or Amazon Resource Name of the stack whose template to get.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stack template, or an error when the backend cannot be reached.</returns>
    Task<Result<CloudFormationStackTemplate>> GetTemplateAsync(
        string stackName, CancellationToken cancellationToken);

    /// <summary>
    /// List the resources a single stack manages.
    /// </summary>
    /// <param name="stackName">The name or Amazon Resource Name of the stack whose resources to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stack resources, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<StackResource>>> ListStackResourcesAsync(
        string stackName, CancellationToken cancellationToken);

    /// <summary>
    /// List the chronological events recorded against a single stack, newest first.
    /// </summary>
    /// <param name="stackName">The name or Amazon Resource Name of the stack whose events to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stack events, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<StackEvent>>> DescribeStackEventsAsync(
        string stackName, CancellationToken cancellationToken);

    /// <summary>
    /// Validate a CloudFormation template supplied either inline or by S3 URL.
    /// </summary>
    /// <param name="templateBody">The inline template body to validate, or <see langword="null"/> when validating by URL.</param>
    /// <param name="templateUrl">The S3 URL of the template to validate, or <see langword="null"/> when validating an inline body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The validation result, or an error when the template is invalid or the backend cannot be reached.</returns>
    Task<Result<TemplateValidationResult>> ValidateTemplateAsync(
        string? templateBody,
        string? templateUrl,
        CancellationToken cancellationToken);

    /// <summary>
    /// Create a new stack from a template body with optional parameters and capabilities.
    /// </summary>
    /// <param name="stackName">The name of the stack to create.</param>
    /// <param name="templateBody">The inline template body that defines the stack, or <see langword="null"/> when creating from a URL.</param>
    /// <param name="templateUrl">The S3 URL of the template that defines the stack, or <see langword="null"/> when creating from an inline body.</param>
    /// <param name="parameters">The input parameters to deploy the stack with.</param>
    /// <param name="capabilities">The capabilities the template requires, such as CAPABILITY_IAM.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The Amazon Resource Name of the created stack, or an error when the create fails.</returns>
    Task<Result<string>> CreateStackAsync(
        string stackName,
        string? templateBody,
        string? templateUrl,
        IReadOnlyList<StackParameter> parameters,
        IReadOnlyList<string> capabilities,
        CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing stack with a revised template body, parameters, and capabilities.
    /// </summary>
    /// <param name="stackName">The name or Amazon Resource Name of the stack to update.</param>
    /// <param name="templateBody">The revised template body to apply.</param>
    /// <param name="parameters">The input parameters to apply to the stack.</param>
    /// <param name="capabilities">The capabilities the template requires, such as CAPABILITY_IAM.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The Amazon Resource Name of the updated stack, or an error when the update fails.</returns>
    Task<Result<string>> UpdateStackAsync(
        string stackName,
        string templateBody,
        IReadOnlyList<StackParameter> parameters,
        IReadOnlyList<string> capabilities,
        CancellationToken cancellationToken);

    /// <summary>
    /// Delete a single stack by its name or Amazon Resource Name.
    /// </summary>
    /// <param name="stackName">The name or Amazon Resource Name of the stack to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the delete fails.</returns>
    Task<Result> DeleteStackAsync(
        string stackName, CancellationToken cancellationToken);

    /// <summary>
    /// Create a change set that previews the changes a revised template would make to a stack.
    /// </summary>
    /// <param name="stackName">The name or Amazon Resource Name of the stack the change set targets.</param>
    /// <param name="changeSetName">The name to assign to the change set.</param>
    /// <param name="changeSetType">Whether the change set creates a new stack (<c>CREATE</c>) or updates one (<c>UPDATE</c>).</param>
    /// <param name="templateBody">The template body the change set evaluates.</param>
    /// <param name="parameters">The input parameters to evaluate the change set with.</param>
    /// <param name="capabilities">The capabilities the template requires, such as CAPABILITY_IAM.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The Amazon Resource Name of the created change set, or an error when the create fails.</returns>
    Task<Result<string>> CreateChangeSetAsync(
        string stackName,
        string changeSetName,
        string changeSetType,
        string templateBody,
        IReadOnlyList<StackParameter> parameters,
        IReadOnlyList<string> capabilities,
        CancellationToken cancellationToken);

    /// <summary>
    /// List the change sets pending against a single stack.
    /// </summary>
    /// <param name="stackName">The name or Amazon Resource Name of the stack whose change sets to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The change sets, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<ChangeSetSummary>>> ListChangeSetsAsync(
        string stackName, CancellationToken cancellationToken);

    /// <summary>
    /// Describe a single change set, including the resource changes it would apply.
    /// </summary>
    /// <param name="stackName">The name or Amazon Resource Name of the stack the change set targets.</param>
    /// <param name="changeSetName">The name or Amazon Resource Name of the change set to describe.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The change set detail, or an error when the backend cannot be reached.</returns>
    Task<Result<ChangeSetDetail>> DescribeChangeSetAsync(
        string stackName, string changeSetName, CancellationToken cancellationToken);

    /// <summary>
    /// Execute a change set, applying its changes to the target stack.
    /// </summary>
    /// <param name="stackName">The name or Amazon Resource Name of the stack the change set targets.</param>
    /// <param name="changeSetName">The name or Amazon Resource Name of the change set to execute.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the execute fails.</returns>
    Task<Result> ExecuteChangeSetAsync(
        string stackName, string changeSetName, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a single change set without applying its changes.
    /// </summary>
    /// <param name="stackName">The name or Amazon Resource Name of the stack the change set targets.</param>
    /// <param name="changeSetName">The name or Amazon Resource Name of the change set to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the delete fails.</returns>
    Task<Result> DeleteChangeSetAsync(
        string stackName, string changeSetName, CancellationToken cancellationToken);

    /// <summary>
    /// Start a drift detection operation against a single stack.
    /// </summary>
    /// <param name="stackName">The name or Amazon Resource Name of the stack to detect drift on.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The id of the drift detection operation, or an error when the start fails.</returns>
    Task<Result<string>> DetectStackDriftAsync(
        string stackName, CancellationToken cancellationToken);

    /// <summary>
    /// Get the status of a running or completed drift detection operation.
    /// </summary>
    /// <param name="driftDetectionId">The id of the drift detection operation to poll.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The drift detection status, or an error when the backend cannot be reached.</returns>
    Task<Result<StackDriftStatus>> DescribeStackDriftDetectionStatusAsync(
        string driftDetectionId, CancellationToken cancellationToken);

    /// <summary>
    /// List the per-resource drift results recorded against a single stack.
    /// </summary>
    /// <param name="stackName">The name or Amazon Resource Name of the stack whose resource drifts to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The per-resource drifts, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<StackResourceDrift>>> DescribeStackResourceDriftsAsync(
        string stackName, CancellationToken cancellationToken);

    /// <summary>
    /// List the exported output values published across all stacks for cross-stack references.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The exports, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<StackExport>>> ListExportsAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// List the names of the stacks that import a single exported value.
    /// </summary>
    /// <param name="exportName">The export name whose importing stacks to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The names of the importing stacks, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<string>>> ListImportsAsync(
        string exportName, CancellationToken cancellationToken);
}
