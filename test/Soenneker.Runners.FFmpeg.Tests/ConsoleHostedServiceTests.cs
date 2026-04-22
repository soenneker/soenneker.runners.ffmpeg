using Soenneker.Tests.HostedUnit;

namespace Soenneker.Runners.FFmpeg.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class ConsoleHostedServiceTests : HostedUnitTest
{
    public ConsoleHostedServiceTests(Host host) : base(host)
    {
    }

    [Test]
    public void Default()
    {

    }
}
