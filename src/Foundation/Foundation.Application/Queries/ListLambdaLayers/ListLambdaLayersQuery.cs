using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Lambda;

namespace Foundation.Application.Queries.ListLambdaLayers;

/// <summary>
/// List the layer versions attached to a Lambda function.
/// </summary>
/// <param name="FunctionName">The name of the function whose layers to list.</param>
public record ListLambdaLayersQuery(string FunctionName) : IQuery<ListLambdaLayersQueryResult>;

/// <summary>
/// The layer versions attached to a Lambda function.
/// </summary>
/// <param name="Layers">The attached layers, ordered by their ARN.</param>
public record ListLambdaLayersQueryResult(
    IReadOnlyList<LambdaLayer> Layers);
