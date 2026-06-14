using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.DeleteSesIdentity;
using Foundation.Application.Commands.EnableDomainDkim;
using Foundation.Application.Commands.VerifyDomainIdentity;
using Foundation.Application.Commands.VerifyEmailIdentity;
using Foundation.Application.Queries.GetSesDomainSetup;
using Foundation.Application.Queries.GetSesIdentity;
using Foundation.Application.Queries.ListSesIdentities;
using Foundation.Domain.Ses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class SesControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<SesController> _logger =
        Substitute.For<ILogger<SesController>>();

    private SesController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListIdentities_WhenQuerySucceeds_ReturnsOkWithIdentities()
    {
        // Arrange
        IReadOnlyList<SesIdentity> identities =
        [
            new("sender@example.com", "EmailAddress", "Success"),
        ];
        _sender
            .Send(Arg.Any<ListSesIdentitiesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSesIdentitiesQueryResult>>(
                new ListSesIdentitiesQueryResult(identities)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListIdentities(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SesIdentityListResponse>>().Subject;
        var identity = ok.Value!.Identities.Should().ContainSingle().Subject;
        identity.Identity.Should().Be("sender@example.com");
        identity.IdentityType.Should().Be("EmailAddress");
        identity.VerificationStatus.Should().Be("Success");
    }

    [Fact]
    public async Task ListIdentities_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListSesIdentitiesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListSesIdentitiesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListIdentities(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetIdentity_WhenQuerySucceeds_ReturnsOkWithDetail()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetSesIdentityQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSesIdentityQueryResult>>(
                new GetSesIdentityQueryResult(
                    new SesIdentityDetail("sender@example.com", "EmailAddress", "Pending"))));
        var sut = CreateSut();

        // Act
        var result = await sut.GetIdentity("sender@example.com", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SesIdentityDetailResponse>>().Subject;
        ok.Value!.Identity.Should().Be("sender@example.com");
        ok.Value.IdentityType.Should().Be("EmailAddress");
        ok.Value.VerificationStatus.Should().Be("Pending");
    }

    [Fact]
    public async Task GetIdentity_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetSesIdentityQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSesIdentityQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetIdentity("sender@example.com", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task VerifyEmailIdentity_WhenCommandSucceeds_ReturnsCreated()
    {
        // Arrange
        _sender
            .Send(Arg.Any<VerifyEmailIdentityCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.VerifyEmailIdentity(
            new SesVerifyEmailRequest("sender@example.com"), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Created>();
        await _sender.Received(1).Send(
            Arg.Is<VerifyEmailIdentityCommand>(command => command.EmailAddress == "sender@example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyEmailIdentity_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<VerifyEmailIdentityCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.VerifyEmailIdentity(
            new SesVerifyEmailRequest("sender@example.com"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteIdentity_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteSesIdentityCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteIdentity("sender@example.com", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<DeleteSesIdentityCommand>(command => command.Identity == "sender@example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteIdentity_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteSesIdentityCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteIdentity("sender@example.com", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetDomainSetup_WhenQuerySucceeds_ReturnsOkWithSetup()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetSesDomainSetupQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSesDomainSetupQueryResult>>(
                new GetSesDomainSetupQueryResult(
                    new SesDomainSetup("example.com", "Pending", "token", "NotStarted", ["a"]))));
        var sut = CreateSut();

        // Act
        var result = await sut.GetDomainSetup("example.com", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SesDomainSetupResponse>>().Subject;
        ok.Value!.Domain.Should().Be("example.com");
        ok.Value.VerificationStatus.Should().Be("Pending");
        ok.Value.VerificationToken.Should().Be("token");
        ok.Value.DkimVerificationStatus.Should().Be("NotStarted");
        ok.Value.DkimTokens.Should().ContainSingle();
    }

    [Fact]
    public async Task GetDomainSetup_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetSesDomainSetupQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetSesDomainSetupQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetDomainSetup("example.com", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task VerifyDomainIdentity_WhenCommandSucceeds_ReturnsCreated()
    {
        // Arrange
        _sender
            .Send(Arg.Any<VerifyDomainIdentityCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.VerifyDomainIdentity(
            new SesVerifyDomainRequest("example.com"), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Created>();
        await _sender.Received(1).Send(
            Arg.Is<VerifyDomainIdentityCommand>(command => command.Domain == "example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyDomainIdentity_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<VerifyDomainIdentityCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.VerifyDomainIdentity(
            new SesVerifyDomainRequest("example.com"), TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task EnableDomainDkim_WhenCommandSucceeds_ReturnsNoContent()
    {
        // Arrange
        _sender
            .Send(Arg.Any<EnableDomainDkimCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.EnableDomainDkim("example.com", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        await _sender.Received(1).Send(
            Arg.Is<EnableDomainDkimCommand>(command => command.Domain == "example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnableDomainDkim_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<EnableDomainDkimCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.EnableDomainDkim("example.com", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
