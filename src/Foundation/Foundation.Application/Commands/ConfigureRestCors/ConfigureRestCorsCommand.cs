using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.ConfigureRestCors;

/// <summary>
/// Configure the CORS policy on an API Gateway REST API resource by wiring an OPTIONS preflight
/// method backed by a MOCK integration that returns the configured Access-Control headers.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API the resource belongs to.</param>
/// <param name="ResourceId">The identifier of the resource to configure.</param>
/// <param name="AllowOrigins">The origins to allow.</param>
/// <param name="AllowMethods">The HTTP methods to allow.</param>
/// <param name="AllowHeaders">The request headers to allow.</param>
public record ConfigureRestCorsCommand(
    string RestApiId,
    string ResourceId,
    IReadOnlyList<string> AllowOrigins,
    IReadOnlyList<string> AllowMethods,
    IReadOnlyList<string> AllowHeaders) : ICommand;
