using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Soenneker.Runners.FFmpeg.Utils.Abstract;

namespace Soenneker.Runners.FFmpeg;

public class ConsoleHostedService : IHostedService
{
    private readonly ILogger<ConsoleHostedService> _logger;

    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IFileOperationsUtil _fileOperationsUtil;
    private readonly IDownloadUtil _downloadUtil;
    private readonly IExtractionUtil _extractionUtil;

    private int? _exitCode;

    public ConsoleHostedService(ILogger<ConsoleHostedService> logger, IHostApplicationLifetime appLifetime, 
        IFileOperationsUtil fileOperationsUtil,IDownloadUtil downloadUtil, IExtractionUtil extractionUtil)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _fileOperationsUtil = fileOperationsUtil;
        _downloadUtil = downloadUtil;
        _extractionUtil = extractionUtil;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                _logger.LogInformation("Running console hosted service ...");

                try
                {
                    string fileName = await _downloadUtil.Download(cancellationToken);

                    string extractionPath = await _extractionUtil.Extract7Zip(fileName, "ffmpeg.exe");

                    await _fileOperationsUtil.Process(Path.Combine(extractionPath, "bin", "ffmpeg.exe"), cancellationToken);

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