using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using PC1databaseCreator.Common.Library.Core.Storage;
using PC1databaseCreator.Common.Library.Core.Storage.Models;
using Serilog;
using PC1databaseCreator.Core.Storage.Models;

namespace PC1databaseCreator
{
    internal static class Program
    {
        /// <summary>
        /// ���ø����̼��� �� �������Դϴ�.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Windows Forms �ʱ�ȭ
            ApplicationConfiguration.Initialize();

            // Serilog ����
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/pc1dbcreator_.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31)
                .CreateLogger();

            try
            {
                var host = CreateHostBuilder().Build();
                ServiceProvider = host.Services;

                // ���� �� ����
                Application.Run(ServiceProvider.GetRequiredService<MainForm>());
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "���ø����̼��� ����ġ �ʰ� ����Ǿ����ϴ�.");
                MessageBox.Show(
                    $"ġ������ ������ �߻��߽��ϴ�.\n{ex.Message}",
                    "����",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// ���� ������ �Ӽ�
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// ȣ��Ʈ ���� ���� �� ���� ����
        /// </summary>
        static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    // �⺻ ���� ���
                    services.AddSingleton<IFileSystem, FileSystem>();
                    services.AddSingleton<IConfiguration>(context.Configuration);

                    // ���丮�� ���� ���� ���
                    services.AddSingleton<HDDConfig>();
                    services.AddSingleton<StorageMetrics>();

                    // �� ���
                    services.AddSingleton<MainForm>();

                    // �α� ���� ���
                    services.AddLogging(builder =>
                    {
                        builder.ClearProviders();
                        builder.AddSerilog(dispose: true);
                    });

                    // �߰� ���� �ε�
                    services.Configure<StorageOptions>(
                        context.Configuration.GetSection("Storage"));
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables();
                });
        }
    }

    /// <summary>
    /// ���丮�� ���� �ɼ� Ŭ����
    /// </summary>
    public class StorageOptions
    {
        public string BasePath { get; set; }
        public int CacheSize { get; set; }
        public int BackupInterval { get; set; }
        public string[] AllowedDrives { get; set; }
    }
}