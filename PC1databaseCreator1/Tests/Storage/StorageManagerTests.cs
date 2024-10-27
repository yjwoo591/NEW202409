```csharp
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;
using PC1databaseCreator.Core.Storage;
using PC1databaseCreator.Core.Storage.Base.Interfaces;
using PC1databaseCreator.Core.Storage.Monitoring;

namespace PC1databaseCreator.Tests.Storage
{
    public class StorageManagerTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly string _testDirectory;
        private readonly StorageManager _storageManager;
        private readonly IStorageMetrics _metrics;
        private readonly ILogger<StorageManager> _logger;

        public StorageManagerTests(ITestOutputHelper output)
        {
            _output = output;

            // 테스트 디렉토리 설정
            _testDirectory = Path.Combine(Path.GetTempPath(), "StorageTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);

            // 로거 설정
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .AddDebug()
                    .AddXUnit(output);
            });

            _logger = loggerFactory.CreateLogger<StorageManager>();
            _metrics = new StorageMetricsCollector(loggerFactory.CreateLogger<StorageMetricsCollector>());
            _storageManager = new StorageManager(_logger, _metrics);

            _output.WriteLine($"Test directory created at: {_testDirectory}");
        }

        [Fact(DisplayName = "파일 쓰기 테스트")]
        public async Task WriteFile_ShouldSucceed()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "test.txt");
            var content = "Test Content";
            var data = Encoding.UTF8.GetBytes(content);

            // Act
            var result = await _storageManager.WriteFileAsync(filePath, data);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(File.Exists(filePath));
            var savedContent = await File.ReadAllTextAsync(filePath);
            Assert.Equal(content, savedContent);

            _output.WriteLine($"File successfully written to: {filePath}");
        }

        [Fact(DisplayName = "파일 읽기 테스트")]
        public async Task ReadFile_ShouldSucceed()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "read_test.txt");
            var content = "Test Content For Reading";
            await File.WriteAllTextAsync(filePath, content);

            // Act
            var result = await _storageManager.ReadFileAsync(filePath);

            // Assert
            Assert.True(result.IsSuccess);
            var readContent = Encoding.UTF8.GetString(result.Data);
            Assert.Equal(content, readContent);

            _output.WriteLine($"File successfully read from: {filePath}");
        }

        [Fact(DisplayName = "파일 삭제 테스트")]
        public async Task DeleteFile_ShouldSucceed()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "delete_test.txt");
            await File.WriteAllTextAsync(filePath, "Test Content For Deletion");

            // Act
            var result = await _storageManager.DeleteFileAsync(filePath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(File.Exists(filePath));

            _output.WriteLine($"File successfully deleted: {filePath}");
        }

        [Fact(DisplayName = "존재하지 않는 파일 읽기 시 실패")]
        public async Task ReadNonExistentFile_ShouldFail()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "non_existent.txt");

            // Act & Assert
            var result = await _storageManager.ReadFileAsync(filePath);
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.ErrorMessage);

            _output.WriteLine($"Expected failure when reading non-existent file: {filePath}");
            _output.WriteLine($"Error message: {result.ErrorMessage}");
        }

        [Fact(DisplayName = "메트릭스 수집 테스트")]
        public async Task MetricsCollection_ShouldTrackOperations()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "metrics_test.txt");
            var content = "Test Content For Metrics";
            var data = Encoding.UTF8.GetBytes(content);

            // Act
            await _storageManager.WriteFileAsync(filePath, data);
            await _storageManager.ReadFileAsync(filePath);
            await _storageManager.DeleteFileAsync(filePath);

            // Assert
            var report = await _metrics.GenerateReportAsync(
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(1));

            Assert.Equal(3, report.TotalOperations);
            Assert.True(report.SuccessfulOperations > 0);

            _output.WriteLine($"Total operations: {report.TotalOperations}");
            _output.WriteLine($"Successful operations: {report.SuccessfulOperations}");
            _output.WriteLine($"Average operation time: {report.AverageOperationTime}ms");
        }

        [Theory(DisplayName = "다양한 크기의 파일 처리")]
        [InlineData(100)]      // 100 bytes
        [InlineData(1024)]     // 1 KB
        [InlineData(1048576)]  // 1 MB
        public async Task HandleDifferentFileSizes_ShouldSucceed(int size)
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, $"size_test_{size}.dat");
            var data = new byte[size];
            new Random().NextBytes(data);

            // Act
            var writeResult = await _storageManager.WriteFileAsync(filePath, data);
            var readResult = await _storageManager.ReadFileAsync(filePath);

            // Assert
            Assert.True(writeResult.IsSuccess);
            Assert.True(readResult.IsSuccess);
            Assert.Equal(data.Length, readResult.Data.Length);

            _output.WriteLine($"Successfully handled file of size: {size} bytes");
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                    _output.WriteLine($"Test directory cleaned up: {_testDirectory}");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error during cleanup: {ex.Message}");
            }

            _storageManager.Dispose();
        }
    }
}