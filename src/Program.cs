using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Soenneker.Enums.DeployEnvironment;
using Soenneker.Extensions.LoggerConfiguration;

namespace Soenneker.Runners.FFmpeg;

public class Program
{
    private static string? _environment;

    public static async Task Main(string[] args)
    {
        _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (string.IsNullOrWhiteSpace(_environment))
            throw new Exception("ASPNETCORE_ENVIRONMENT is not set");

        try
        {
            await CreateHostBuilder(args).RunConsoleAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Stopped program because of exception");
            throw;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    /// <summary>
    /// Used for WebApplicationFactory, cannot delete, cannot change access, cannot change number of parameters.
    /// </summary>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        DeployEnvironment envEnum = DeployEnvironment.FromName(_environment);

        LoggerConfigExtension.BuildBootstrapLoggerAndSetGlobally(envEnum);

        IHostBuilder? host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, builder) =>
            {
                builder.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);

                IConfigurationRoot configurationRoot = builder.Build();
            })
            .UseSerilog()
            .ConfigureServices((hostContext, services) =>
            {
                Startup.ConfigureServices(services);
            });

        return host;
    }
}