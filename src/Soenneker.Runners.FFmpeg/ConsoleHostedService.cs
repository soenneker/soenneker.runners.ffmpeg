using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Soenneker.Compression.SevenZip.Abstract;
using Soenneker.Managers.Runners.Abstract;
using Soenneker.Utils.Directory.Abstract;
using Soenneker.Utils.File.Download.Abstract;

namespace Soenneker.Runners.FFmpeg;

public sealed class ConsoleHostedService : IHostedService
{
    private readonly ILogger<ConsoleHostedService> _logger;

    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IRunnersManager _runnersManager;
    private readonly ISevenZipCompressionUtil _sevenZipCompressionUtil;
    private readonly IFileDownloadUtil _fileDownloadUtil;
    private readonly IDirectoryUtil _directoryUtil;

    private int? _exitCode;

    public ConsoleHostedService(ILogger<ConsoleHostedService> logger, IHostApplicationLifetime appLifetime, IRunnersManager runnersManager,
        ISevenZipCompressionUtil sevenZipCompressionUtil, IFileDownloadUtil fileDownloadUtil, IDirectoryUtil directoryUtil)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _runnersManager = runnersManager;
        _sevenZipCompressionUtil = sevenZipCompressionUtil;
        _fileDownloadUtil = fileDownloadUtil;
        _directoryUtil = directoryUtil;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _appLifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                _logger.LogInformation("Running console hosted service ...");

                try
                {
                    string? filePath = await _fileDownloadUtil.Download("https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-full.7z", fileExtension: ".7z",
                        cancellationToken: cancellationToken);

                    string extractionPath = await _sevenZipCompressionUtil.Extract(filePath!, cancellationToken);

                    await _directoryUtil.MoveContentsUpOneLevelStrict(extractionPath, cancellationToken);

                    await _runnersManager.PushIfChangesNeeded(Path.Combine(extractionPath, "bin", Constants.FileName), Constants.FileName, Constants.Library,
                        $"https://github.com/soenneker/{Constants.Library}", false, cancellationToken);

                    _logger.LogInformation("Complete!");

                    _exitCode = 0;
                }
                catch (Exception e)
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();

                    _logger.LogError(e, "Unhandled exception");

                    await Task.Delay(2000, cancellationToken);
                    _exitCode = 1;
                }
                finally
                {
                    // Stop the application once the work is done
                    _appLifetime.StopApplication();
                }
            }, cancellationToken);
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Exiting with return code: {exitCode}", _exitCode);

        // Exit code may be null if the user cancelled via Ctrl+C/SIGTERM
        Environment.ExitCode = _exitCode.GetValueOrDefault(-1);
        return Task.CompletedTask;
    }
}