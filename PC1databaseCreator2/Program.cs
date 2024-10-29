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
        /// 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Windows Forms 초기화
            ApplicationConfiguration.Initialize();

            // Serilog 설정
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

                // 메인 폼 실행
                Application.Run(ServiceProvider.GetRequiredService<MainForm>());
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "애플리케이션이 예기치 않게 종료되었습니다.");
                MessageBox.Show(
                    $"치명적인 오류가 발생했습니다.\n{ex.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// 서비스 제공자 속성
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// 호스트 빌더 생성 및 서비스 구성
        /// </summary>
        static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    // 기본 서비스 등록
                    services.AddSingleton<IFileSystem, FileSystem>();
                    services.AddSingleton<IConfiguration>(context.Configuration);

                    // 스토리지 관련 서비스 등록
                    services.AddSingleton<HDDConfig>();
                    services.AddSingleton<StorageMetrics>();

                    // 폼 등록
                    services.AddSingleton<MainForm>();

                    // 로깅 서비스 등록
                    services.AddLogging(builder =>
                    {
                        builder.ClearProviders();
                        builder.AddSerilog(dispose: true);
                    });

                    // 추가 구성 로드
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
    /// 스토리지 설정 옵션 클래스
    /// </summary>
    public class StorageOptions
    {
        public string BasePath { get; set; }
        public int CacheSize { get; set; }
        public int BackupInterval { get; set; }
        public string[] AllowedDrives { get; set; }
    }
}