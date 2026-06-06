using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateRestDeployment;

/// <summary>
/// Create a deployment of an API Gateway REST API, optionally creating a stage that points at it.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the deployment belongs to.</param>
/// <param name="StageName">An optional stage name to create and point at the new deployment, or <c>null</c> to create the deployment only.</param>
/// <param name="Description">An optional human-readable description.</param>
public record CreateRestDeploymentCommand(
    string RestApiId,
    string? StageName,
    string? Description) : ICommand<string>;
