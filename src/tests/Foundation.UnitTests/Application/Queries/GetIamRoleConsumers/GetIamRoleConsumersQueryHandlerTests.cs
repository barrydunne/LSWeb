using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.GetIamRoleConsumers;
using Foundation.Domain.Iam;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetIamRoleConsumers;

public class GetIamRoleConsumersQueryHandlerTests
{
    private const string RoleArn = "arn:aws:iam::000000000000:role/LambdaExec";

    private readonly IIamClient _iamClient = Substitute.For<IIamClient>();
    private readonly ILambdaClient _lambdaClient = Substitute.For<ILambdaClient>();

    private GetIamRoleConsumersQueryHandler CreateSut()
        => new(_iamClient, _lambdaClient, NullLogger<GetIamRoleConsumersQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    private static IamRoleDetail? Role(string arn)
        => new(
            "LambdaExec",
            arn,
            "AROA1",
            "/",
            DateTimeOffset.UtcNow,
            "Lambda execution role",
            3600,
            "{\"Version\":\"2012-10-17\",\"Statement\":[]}",
            [],
            [],
            [],
            null);

    private static LambdaFunctionSummary Summary(string name)
        => new(name, "python3.12", "desc", "2024-01-01", 128, 30);

    private static LambdaFunctionDetail Detail(string name, string role)
        => new(name, $"arn:aws:lambda:::function:{name}", "python3.12", "app.handler", "desc", "2024-01-01", 128, 30, role);

    [Fact]
    public async Task Handle_WhenLambdaUsesRole_ReturnsConsumer()
    {
        // Arrange
        _iamClient.GetRoleAsync("LambdaExec", Arg.Any<CancellationToken>()).Returns(Ok(Role(RoleArn)));
        _lambdaClient
            .ListFunctionsAsync(Arg.Any<CancellationToken>())
            .Returns(Ok<IReadOnlyList<LambdaFunctionSummary>>([Summary("orders")]));
        _lambdaClient.GetFunctionAsync("orders", Arg.Any<CancellationToken>()).Returns(Ok(Detail("orders", RoleArn)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamRoleConsumersQuery("LambdaExec"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Consumers.Should().ContainSingle();
        var consumer = result.Value.Consumers[0];
        consumer.ConsumerType.Should().Be("Lambda function");
        consumer.ResourceName.Should().Be("orders");
        consumer.ServiceKey.Should().Be("lambda");
    }

    [Fact]
    public async Task Handle_WhenLambdaUsesDifferentRole_FiltersItOut()
    {
        // Arrange
        _iamClient.GetRoleAsync("LambdaExec", Arg.Any<CancellationToken>()).Returns(Ok(Role(RoleArn)));
        _lambdaClient
            .ListFunctionsAsync(Arg.Any<CancellationToken>())
            .Returns(Ok<IReadOnlyList<LambdaFunctionSummary>>([Summary("orders")]));
        _lambdaClient
            .GetFunctionAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Ok(Detail("orders", "arn:aws:iam::000000000000:role/Other")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamRoleConsumersQuery("LambdaExec"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Consumers.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFunctionDetailFails_SkipsThatFunction()
    {
        // Arrange
        _iamClient.GetRoleAsync("LambdaExec", Arg.Any<CancellationToken>()).Returns(Ok(Role(RoleArn)));
        _lambdaClient
            .ListFunctionsAsync(Arg.Any<CancellationToken>())
            .Returns(Ok<IReadOnlyList<LambdaFunctionSummary>>([Summary("broken"), Summary("orders")]));
        _lambdaClient
            .GetFunctionAsync("broken", Arg.Any<CancellationToken>())
            .Returns(new Error("detail boom"));
        _lambdaClient.GetFunctionAsync("orders", Arg.Any<CancellationToken>()).Returns(Ok(Detail("orders", RoleArn)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamRoleConsumersQuery("LambdaExec"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Consumers.Should().ContainSingle();
        result.Value.Consumers[0].ResourceName.Should().Be("orders");
    }

    [Fact]
    public async Task Handle_WhenRoleLookupFails_PropagatesError()
    {
        // Arrange
        _iamClient.GetRoleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new Error("role boom"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamRoleConsumersQuery("missing"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("role boom");
    }

    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ReturnsNoConsumers()
    {
        // Arrange
        _iamClient
            .GetRoleAsync("missing", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IamRoleDetail?>>((IamRoleDetail?)null));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamRoleConsumersQuery("missing"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Consumers.Should().BeEmpty();
        await _lambdaClient.DidNotReceive().ListFunctionsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenListFunctionsFails_PropagatesError()
    {
        // Arrange
        _iamClient.GetRoleAsync("LambdaExec", Arg.Any<CancellationToken>()).Returns(Ok(Role(RoleArn)));
        _lambdaClient.ListFunctionsAsync(Arg.Any<CancellationToken>()).Returns(new Error("list boom"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetIamRoleConsumersQuery("LambdaExec"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("list boom");
    }
}
