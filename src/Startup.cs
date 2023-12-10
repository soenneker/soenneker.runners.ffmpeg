using Microsoft.Extensions.DependencyInjection;
using Soenneker.Git.Util.Registrars;
using Soenneker.Runners.FFmpeg.Utils;
using Soenneker.Runners.FFmpeg.Utils.Abstract;
using Soenneker.Utils.Dotnet.NuGet.Registrars;
using Soenneker.Utils.Dotnet.Registrars;
using Soenneker.Utils.File.Registrars;
using Soenneker.Utils.FileSync.Registrars;
using Soenneker.Utils.HttpClientCache.Registrar;
using Soenneker.Utils.SHA3.Registrars;

namespace Soenneker.Runners.FFmpeg;

/// <summary>
/// Console type startup
/// </summary>
public static class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public static void ConfigureServices(IServiceCollection services)
    {
        services.SetupIoC();
    }

    public static void SetupIoC(this IServiceCollection services)
    {
        services.AddHttpClientCache();
        services.AddHostedService<ConsoleHostedService>();
        services.AddFileUtilAsScoped();
        services.AddSha3UtilAsScoped();
        services.AddFileUtilSyncAsScoped();
        services.AddGitUtilAsScoped();
        services.AddScoped<IExtractionUtil, ExtractionUtil>();
        services.AddScoped<IDownloadUtil, DownloadUtil>();
        services.AddScoped<IFileOperationsUtil, FileOperationsUtil>();
        services.AddDotnetNuGetUtilAsScoped();
        services.AddDotnetUtilAsScoped();
    }
}