using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Lambda;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetIamRoleConsumers;

internal sealed partial class GetIamRoleConsumersQueryHandler
    : IQueryHandler<GetIamRoleConsumersQuery, GetIamRoleConsumersQueryResult>
{
    private readonly IIamClient _iamClient;
    private readonly ILambdaClient _lambdaClient;
    private readonly ILogger _logger;

    public GetIamRoleConsumersQueryHandler(
        IIamClient iamClient,
        ILambdaClient lambdaClient,
        ILogger<GetIamRoleConsumersQueryHandler> logger)
    {
        _iamClient = iamClient;
        _lambdaClient = lambdaClient;
        _logger = logger;
    }

    public async Task<Result<GetIamRoleConsumersQueryResult>> Handle(
        GetIamRoleConsumersQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.RoleName);

        var role = await _iamClient.GetRoleAsync(request.RoleName, cancellationToken);
        if (!role.IsSuccess)
        {
            Result<GetIamRoleConsumersQueryResult> failure = role.Error!.Value;
            return failure;
        }

        var functions = await _lambdaClient.ListFunctionsAsync(cancellationToken);
        if (!functions.IsSuccess)
        {
            Result<GetIamRoleConsumersQueryResult> failure = functions.Error!.Value;
            return failure;
        }

        var roleArn = role.Value.Arn;
        var consumers = new List<IamRoleConsumer>();
        foreach (var function in functions.Value)
        {
            var detail = await _lambdaClient.GetFunctionAsync(function.FunctionName, cancellationToken);
            if (!detail.IsSuccess)
                continue;

            if (string.Equals(detail.Value.Role, roleArn, StringComparison.OrdinalIgnoreCase))
                consumers.Add(new IamRoleConsumer("Lambda function", function.FunctionName, "lambda"));
        }

        LogHandled(consumers.Count);
        return new GetIamRoleConsumersQueryResult(consumers);
    }

    [LoggerMessage(LogLevel.Trace, "Finding consumers of IAM role {RoleName}.")]
    private partial void LogHandling(string roleName);

    [LoggerMessage(LogLevel.Trace, "IAM role consumer lookup handled. Consumer count: {Count}")]
    private partial void LogHandled(int count);
}
