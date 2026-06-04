using System.Diagnostics.CodeAnalysis;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Foundation.Infrastructure.Aws;
using DomainRestApi = Foundation.Domain.ApiGateway.RestApi;
using SdkRestApi = Amazon.APIGateway.Model.RestApi;

namespace Foundation.Infrastructure.ApiGateway;

/// <summary>
/// Reads API Gateway through the resilient AWS gateway so the same code works against LocalStack or
/// real AWS. All access flows through <see cref="IAwsGateway"/>, which records capability and
/// converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class ApiGatewayClientAdapter : IApiGatewayClient
{
    private const string ServiceKey = "apigateway";

    private readonly IAwsGateway _gateway;

    public ApiGatewayClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<DomainRestApi>>> ListRestApisAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, IReadOnlyList<DomainRestApi>>(
            ServiceKey,
            async (client, token) =>
            {
                var restApis = new List<DomainRestApi>();
                string? position = null;

                do
                {
                    var response = await client.GetRestApisAsync(
                        new GetRestApisRequest { Position = position },
                        token);

                    foreach (var item in response.Items ?? [])
                        restApis.Add(ToRestApi(item));

                    position = response.Position;
                }
                while (!string.IsNullOrEmpty(position));

                return restApis;
            },
            cancellationToken);

    private static DomainRestApi ToRestApi(SdkRestApi item)
        => new(
            item.Id ?? string.Empty,
            item.Name ?? string.Empty,
            string.IsNullOrWhiteSpace(item.Description) ? null : item.Description,
            item.CreatedDate is { } created
                ? new DateTimeOffset(created.ToUniversalTime(), TimeSpan.Zero)
                : null);
}
