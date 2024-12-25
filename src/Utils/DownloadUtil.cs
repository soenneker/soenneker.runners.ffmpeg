using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Runners.FFmpeg.Utils.Abstract;
using Soenneker.Utils.FileSync;
using Soenneker.Utils.HttpClientCache.Abstract;

namespace Soenneker.Runners.FFmpeg.Utils;

///<inheritdoc cref="IDownloadUtil"/>
public class DownloadUtil : IDownloadUtil
{
    private readonly ILogger<DownloadUtil> _logger;
    private readonly IHttpClientCache _httpClientCache;

    public DownloadUtil(IHttpClientCache httpClientCache, ILogger<DownloadUtil> logger)
    {
        _httpClientCache = httpClientCache;
        _logger = logger;
    }

    public async ValueTask<string> Download(CancellationToken cancellationToken)
    {
        HttpClient client = await _httpClientCache.Get(nameof(DownloadUtil), cancellationToken: cancellationToken);

        const string uri = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-full.7z";

        string tempFile = FileUtilSync.GetTempFileName() + ".7z";

        _logger.LogInformation("Downloading file from uri ({uri}) to fileName ({fileName})...", uri, tempFile);

        HttpResponseMessage response = await client.GetAsync(uri, cancellationToken);

        await using (var fs = new FileStream(tempFile, FileMode.CreateNew))
        {
            await response.Content.CopyToAsync(fs, cancellationToken);
        }

        _logger.LogDebug("Finished downloading file from uri ({uri})", uri);

        return tempFile;
    }
}