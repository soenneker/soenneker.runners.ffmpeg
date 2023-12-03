using Microsoft.Extensions.DependencyInjection;
using Soenneker.Git.Util.Registrars;
using Soenneker.Runners.FFmpeg.Utils;
using Soenneker.Runners.FFmpeg.Utils.Abstract;
using Soenneker.Utils.Dotnet.NuGet.Registrars;
using Soenneker.Utils.Dotnet.Registrars;
using Soenneker.Utils.File.Registrars;
using Soenneker.Utils.HttpClientCache.Registrar;

namespace Soenneker.Runners.FFmpeg;

/// <summary>
/// Console type startup
/// </summary>
public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public static void ConfigureServices(IServiceCollection services)
    {
        SetupIoC(services);
    }

    public static void SetupIoC(IServiceCollection services)
    {
        services.AddHttpClientCache();
        services.AddHostedService<ConsoleHostedService>();
        services.AddFileUtilAsScoped();
        services.AddGitUtilAsScoped();
        services.AddScoped<IExtractionUtil, ExtractionUtil>();
        services.AddScoped<IDownloadUtil, DownloadUtil>();
        services.AddScoped<IFileOperationsUtil, FileOperationsUtil>();
        services.AddDotnetNuGetUtilAsScoped();
        services.AddDotnetUtilAsScoped();
    }
}