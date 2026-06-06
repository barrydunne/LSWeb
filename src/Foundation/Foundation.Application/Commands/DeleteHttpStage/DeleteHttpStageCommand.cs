using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteHttpStage;

/// <summary>
/// Delete an Amazon API Gateway v2 stage. This action cannot be undone.
/// </summary>
/// <param name="ApiId">The identifier of the API the stage belongs to.</param>
/// <param name="StageName">The name of the stage to delete.</param>
public record DeleteHttpStageCommand(string ApiId, string StageName) : ICommand;
