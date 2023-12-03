using System.Threading.Tasks;
using Soenneker.Facts.Local;
using Soenneker.Runners.FFmpeg.Utils.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.Runners.FFmpeg.Tests.Utils;

[Collection("Collection")]
public class DownloadUtilTests : FixturedUnitTest
{
    private readonly IDownloadUtil _util;

    public DownloadUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IDownloadUtil>();
    }

    [LocalFact]
    public async Task Download_should_download()
    {
        string result = await _util.Download();
    }

    [LocalFact]
    public async Task Download_and_extract_should_not_throw()
    {
      //  string downloadedFile = await _util.Download();

        var extractUtil = Resolve<IExtractionUtil>();

        string? directory = await extractUtil.Extract7Zip(@"C:\Users\Jake\AppData\Local\Temp\0cdf5547-eaee-4f5d-badb-ba5845ec1751.7z");
    }
}