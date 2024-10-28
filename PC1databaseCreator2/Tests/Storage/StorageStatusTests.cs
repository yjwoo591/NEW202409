using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PC1databaseCreator.Core.Storage;
using PC1databaseCreator.Core.Storage.Models;
using Xunit;
using Xunit.Abstractions;

namespace PC1databaseCreator.Tests.Storage
{
    public class StorageStatusTests : IDisposable
    {
        private readonly string _testPrimaryPath;
        private readonly string _testMirrorPath;
        private readonly HDDConfig _config;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<HDDManager> _logger;

        public StorageStatusTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = new TestLogger<HDDManager>(testOutputHelper);

            // 테스트용 임시 디렉토리 설정
            _testPrimaryPath = Path.Combine(Path.GetTempPath(), "HDDTest", "Primary");
            _testMirrorPath = Path.Combine(Path.GetTempPath(), "HDDTest", "Mirror");

            // 테스트 설정 구성
            _config = new HDDConfig();
            _config.AddDrivePair(_testPrimaryPath, _testMirrorPath);

            // 테스트 전 이전 테스트 데이터 정리
            CleanupTestDirectories();

            _testOutputHelper.WriteLine($"Test initialized with paths: Primary={_testPrimaryPath}, Mirror={_testMirrorPath}");
        }

        [Fact(DisplayName = "Storage initialization creates required folders")]
        public void HDDManager_InitializeStorage_ShouldCreateRequiredFolders()
        {
            try
            {
                // Arrange & Act
                using var manager = new HDDManager(_config, _logger);
                _testOutputHelper.WriteLine("HDDManager initialized");

                // Assert
                var primaryFastAccess = Path.Combine(_testPrimaryPath, "FastAccess");
                var primaryArchive = Path.Combine(_testPrimaryPath, "Archive");
                var mirrorFastAccess = Path.Combine(_testMirrorPath, "FastAccess");
                var mirrorArchive = Path.Combine(_testMirrorPath, "Archive");

                _testOutputHelper.WriteLine("Verifying directory creation...");

                Xunit.Assert.True(Directory.Exists(primaryFastAccess),
                    $"Primary FastAccess directory should exist at {primaryFastAccess}");
                Xunit.Assert.True(Directory.Exists(primaryArchive),
                    $"Primary Archive directory should exist at {primaryArchive}");
                Xunit.Assert.True(Directory.Exists(mirrorFastAccess),
                    $"Mirror FastAccess directory should exist at {mirrorFastAccess}");
                Xunit.Assert.True(Directory.Exists(mirrorArchive),
                    $"Mirror Archive directory should exist at {mirrorArchive}");

                _testOutputHelper.WriteLine("All directories verified successfully");
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine($"Test failed with error: {ex}");
                throw;
            }
        }

        // ... 다른 테스트 메서드들 ...

        public void Dispose()
        {
            try
            {
                CleanupTestDirectories();
                _testOutputHelper.WriteLine("Test cleanup completed successfully");
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine($"Cleanup failed with error: {ex}");
            }
        }

        private void CleanupTestDirectories()
        {
            if (Directory.Exists(_testPrimaryPath))
            {
                Directory.Delete(_testPrimaryPath, true);
                _testOutputHelper.WriteLine($"Cleaned up primary directory: {_testPrimaryPath}");
            }

            if (Directory.Exists(_testMirrorPath))
            {
                Directory.Delete(_testMirrorPath, true);
                _testOutputHelper.WriteLine($"Cleaned up mirror directory: {_testMirrorPath}");
            }
        }
    }

    /// <summary>
    /// xUnit 테스트 출력을 위한 로거
    /// </summary>
    internal class TestLogger<T> : ILogger<T>
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestLogger(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            _testOutputHelper.WriteLine($"[{logLevel}] {message}");

            if (exception != null)
            {
                _testOutputHelper.WriteLine($"Exception: {exception}");
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
    }

    internal class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();
        private NullScope() { }
        public void Dispose() { }
    }
}