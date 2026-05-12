using Foundation.Infrastructure.Streaming;

namespace Foundation.UnitTests.Infrastructure.Streaming;

public class StreamSessionManagerTests
{
    private readonly StreamSessionManager _sut = new();

    [Fact]
    public void Add_RegistersSessionWithConnectionId()
    {
        _sut.Add("conn-1");

        _sut.Count.Should().Be(1);
        _sut.Sessions.Should().ContainSingle(session => session.ConnectionId == "conn-1");
    }

    [Fact]
    public void Add_SameConnectionTwice_KeepsSingleSession()
    {
        _sut.Add("conn-1");
        _sut.Add("conn-1");

        _sut.Count.Should().Be(1);
    }

    [Fact]
    public void Add_DifferentConnections_TracksEach()
    {
        _sut.Add("conn-1");
        _sut.Add("conn-2");

        _sut.Sessions.Select(session => session.ConnectionId)
            .Should()
            .BeEquivalentTo("conn-1", "conn-2");
    }

    [Fact]
    public void Remove_ExistingConnection_RemovesSession()
    {
        _sut.Add("conn-1");

        _sut.Remove("conn-1");

        _sut.Count.Should().Be(0);
    }

    [Fact]
    public void Remove_UnknownConnection_LeavesSessionsUnchanged()
    {
        _sut.Add("conn-1");

        _sut.Remove("conn-2");

        _sut.Count.Should().Be(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Add_WhenConnectionIdMissing_Throws(string? connectionId)
    {
        var act = () => _sut.Add(connectionId!);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Remove_WhenConnectionIdMissing_Throws(string? connectionId)
    {
        var act = () => _sut.Remove(connectionId!);

        act.Should().Throw<ArgumentException>();
    }
}
