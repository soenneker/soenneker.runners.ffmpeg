using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using Soenneker.Runners.FFmpeg.Utils.Abstract;
using Soenneker.Utils.Directory.Abstract;

namespace Soenneker.Runners.FFmpeg.Utils;

///<inheritdoc cref="IDownloadUtil"/>
public class ExtractionUtil : IExtractionUtil
{
    private readonly ILogger<DownloadUtil> _logger;
    private readonly IDirectoryUtil _directoryUtil;

    public ExtractionUtil(ILogger<DownloadUtil> logger, IDirectoryUtil directoryUtil)
    {
        _logger = logger;
        _directoryUtil = directoryUtil;
    }

    public async ValueTask<string> Extract7Zip(string fileNamePath, string? specificFileFilter = null)
    {
        string tempDir = _directoryUtil.CreateTempDirectory();

        _logger.LogInformation("Extracting file ({file}) to temp dir ({dir})...", fileNamePath, tempDir);

        await using (Stream stream = File.OpenRead(fileNamePath))
        {
            using (var archive = SevenZipArchive.Open(stream))
            {
                foreach (var entry in archive.Entries)
                {
                    try
                    {
                        if (entry.IsDirectory)
                        {
                            string directoryPath = Path.Combine(tempDir, entry.Key);

                            _directoryUtil.CreateIfDoesNotExist(directoryPath);
                        }
                        else
                        {
                            if (specificFileFilter != null)
                            {
                                if (!entry.Key.Contains(specificFileFilter))
                                {
                                    continue;
                                }
                            }

                            _logger.LogInformation("Extracting {message} ({size})...", entry.Key, entry.Size);

                            entry.WriteToFile(Path.Combine(tempDir, entry.Key));
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Exception extracting");
                    }
                }
            }
        }

        _logger.LogInformation("Finished extracting {fileName}", fileNamePath);

        var path = Path.Combine(tempDir, GetFirstDirectory(tempDir));
        return path;
    }

    private static string GetLastPart(string path)
    {
        return path.Split(Path.DirectorySeparatorChar).Last();
    }

    private static string GetFirstDirectory(string path)
    {
        var directory = Directory.GetDirectories(path).First();
        directory = GetLastPart(directory);
        return directory;
    }
}