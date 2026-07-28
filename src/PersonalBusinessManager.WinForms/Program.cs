using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PersonalBusinessManager.Infrastructure;
using PersonalBusinessManager.WinForms.Forms;
using Serilog;

namespace PersonalBusinessManager.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        string applicationDataDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "PersonalBusinessManager");

        string logDirectory = Path.Combine(
            applicationDataDirectory,
            "Logs");

        Directory.CreateDirectory(logDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(
                    logDirectory,
                    "application-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();

        Application.ThreadException += (_, eventArgs) =>
        {
            Log.Error(
                eventArgs.Exception,
                "An unhandled WinForms exception occurred.");

            MessageBox.Show(
                "An unexpected application error occurred. " +
                "The error has been written to the application log.",
                "Application error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        };

        AppDomain.CurrentDomain.UnhandledException +=
            (_, eventArgs) =>
            {
                if (eventArgs.ExceptionObject is Exception exception)
                {
                    Log.Fatal(
                        exception,
                        "An unhandled application exception occurred.");
                }
            };

        try
        {
            HostApplicationBuilder builder =
                Host.CreateApplicationBuilder();

            builder.Logging.ClearProviders();
            builder.Services.AddSerilog();

            string? connectionString =
                Environment.GetEnvironmentVariable(
                    "PBM_CONNECTION_STRING");

            builder.Services.AddInfrastructure(
                connectionString);

            builder.Services.AddSingleton<MainShellForm>();

            using IHost host = builder.Build();

            host.StartAsync()
                .GetAwaiter()
                .GetResult();

            MainShellForm mainForm =
                host.Services.GetRequiredService<
                    MainShellForm>();

            Application.Run(mainForm);

            host.StopAsync()
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception exception)
        {
            Log.Fatal(
                exception,
                "The application failed during startup.");

            MessageBox.Show(
                "The application could not start. " +
                "Check the application log for more information.",
                "Startup error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}