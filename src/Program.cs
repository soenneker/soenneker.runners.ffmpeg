using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Soenneker.Enums.DeployEnvironment;
using Soenneker.Extensions.LoggerConfiguration;

namespace Soenneker.Runners.FFmpeg;

public sealed class Program
{
    private static string? _environment;

    private static CancellationTokenSource? _cts;

    public static async Task Main(string[] args)
    {
        _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (string.IsNullOrWhiteSpace(_environment))
            throw new Exception("ASPNETCORE_ENVIRONMENT is not set");

        // Declare CancellationTokenSource in a broader scope
        _cts = new CancellationTokenSource(); // Use 'using' to ensure proper disposal
        Console.CancelKeyPress += OnCancelKeyPress;

        try
        {
            await CreateHostBuilder(args).RunConsoleAsync(_cts.Token);
        }
        catch (Exception e)
        {
            Log.Error(e, "Stopped program because of exception");
            throw;
        }
        finally
        {
            Console.CancelKeyPress -= OnCancelKeyPress; // Detach the handler

            _cts.Dispose();
            await Log.CloseAndFlushAsync();
        }
    }

    /// <summary>
    /// Used for WebApplicationFactory, cannot delete, cannot change access, cannot change number of parameters.
    /// </summary>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        DeployEnvironment envEnum = DeployEnvironment.FromName(_environment);

        LoggerConfigurationExtension.BuildBootstrapLoggerAndSetGloballySync(envEnum);

        IHostBuilder host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, builder) =>
            {
                builder.AddEnvironmentVariables();
                builder.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);

                builder.Build();
            })
            .UseSerilog()
            .ConfigureServices((_, services) => { Startup.ConfigureServices(services); });

        return host;
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs)
    {
        eventArgs.Cancel = true; // Prevents immediate termination
        _cts?.Cancel();
    }
}