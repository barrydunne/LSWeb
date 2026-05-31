using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Iam;
using Foundation.Application.Queries.GetAccountPasswordPolicy;
using Foundation.Domain.Iam;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetAccountPasswordPolicy;

public class GetAccountPasswordPolicyQueryHandlerTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();

    private GetAccountPasswordPolicyQueryHandler CreateSut()
        => new(_client, NullLogger<GetAccountPasswordPolicyQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenPolicyExists_ReturnsPolicy()
    {
        // Arrange
        var policy = new IamPasswordPolicy(
            MinimumPasswordLength: 14,
            RequireSymbols: true,
            RequireNumbers: true,
            RequireUppercaseCharacters: true,
            RequireLowercaseCharacters: true,
            AllowUsersToChangePassword: true,
            ExpirePasswords: true,
            MaxPasswordAge: 90,
            PasswordReusePrevention: 5,
            HardExpiry: false);
        _client
            .GetAccountPasswordPolicyAsync(Arg.Any<CancellationToken>())
            .Returns(Ok<IamPasswordPolicy?>(policy));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetAccountPasswordPolicyQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var actual = result.Value.Policy!;
        actual.MinimumPasswordLength.Should().Be(policy.MinimumPasswordLength);
        actual.RequireSymbols.Should().Be(policy.RequireSymbols);
        actual.RequireNumbers.Should().Be(policy.RequireNumbers);
        actual.RequireUppercaseCharacters.Should().Be(policy.RequireUppercaseCharacters);
        actual.RequireLowercaseCharacters.Should().Be(policy.RequireLowercaseCharacters);
        actual.AllowUsersToChangePassword.Should().Be(policy.AllowUsersToChangePassword);
        actual.ExpirePasswords.Should().Be(policy.ExpirePasswords);
        actual.MaxPasswordAge.Should().Be(policy.MaxPasswordAge);
        actual.PasswordReusePrevention.Should().Be(policy.PasswordReusePrevention);
        actual.HardExpiry.Should().Be(policy.HardExpiry);
    }

    [Fact]
    public async Task Handle_WhenNoPolicySet_ReturnsNull()
    {
        // Arrange
        _client
            .GetAccountPasswordPolicyAsync(Arg.Any<CancellationToken>())
            .Returns(Ok<IamPasswordPolicy?>(null));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetAccountPasswordPolicyQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Policy.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetAccountPasswordPolicyAsync(Arg.Any<CancellationToken>())
            .Returns(new Error("policy boom"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetAccountPasswordPolicyQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("policy boom");
    }
}
