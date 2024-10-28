using Xunit;
using FluentAssertions;
using Moq;
using PC1databaseCreator.Common.Infrastructure.Interfaces.Logging;
using PC1databaseCreator.Core.Storage.Services;
using PC1databaseCreator.Common.Results;

namespace PC1databaseCreator.Tests.Storage.Infrastructure
{
    public class FileSystemProviderTests : IDisposable
    {
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly string _testPath;
        private readonly StorageMetricsService _metricsService;

        public FileSystemProviderTests()
        {
            _loggerMock = new Mock<ILoggerService>();
            _testPath = Path.Combine(Path.GetTempPath(), "FileSystemTests");
            _metricsService = new StorageMetricsService(_loggerMock.Object);

            if (Directory.Exists(_testPath))
            {
                Directory.Delete(_testPath, true);
            }
            Directory.CreateDirectory(_testPath);
        }

        [Fact]
        public async Task WriteFile_ValidData_ShouldSucceed()
        {
            // Arrange
            var testData = new byte[] { 1, 2, 3, 4, 5 };
            var fileName = "test.dat";
            var fullPath = Path.Combine(_testPath, fileName);

            // Act
            var result = await WriteFileAsync(fileName, testData);

            // Assert
            result.IsSuccess.Should().BeTrue();
            File.Exists(fullPath).Should().BeTrue();
            var savedData = await File.ReadAllBytesAsync(fullPath);
            savedData.Should().BeEquivalentTo(testData);
        }

        [Fact]
        public async Task ReadFile_ExistingFile_ShouldReturnCorrectData()
        {
            // Arrange
            var testData = new byte[] { 1, 2, 3, 4, 5 };
            var fileName = "test.dat";
            var fullPath = Path.Combine(_testPath, fileName);
            await File.WriteAllBytesAsync(fullPath, testData);

            // Act
            var result = await ReadFileAsync(fileName);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(testData);
        }

        [Fact]
        public async Task ReadFile_NonExistingFile_ShouldReturnFailure()
        {
            // Arrange
            var fileName = "nonexistent.dat";

            // Act
            var result = await ReadFileAsync(fileName);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("not found"));
        }

        [Fact]
        public void GetMetrics_ShouldReturnCorrectValues()
        {
            // Act
            _metricsService.RecordOperation(_testPath, 100, true);
            _metricsService.RecordOperation(_testPath, 50, false);
            var metrics = _metricsService.GetMetrics(_testPath);

            // Assert
            metrics.BytesWritten.Should().Be(100);
            metrics.BytesRead.Should().Be(50);
            metrics.WriteOperations.Should().Be(1);
            metrics.ReadOperations.Should().Be(1);
        }

        private async Task<Result<byte[]>> ReadFileAsync(string fileName)
        {
            try
            {
                var fullPath = Path.Combine(_testPath, fileName);
                if (!File.Exists(fullPath))
                {
                    return Result<byte[]>.Failure($"File not found: {fileName}");
                }

                var data = await File.ReadAllBytesAsync(fullPath);
                _metricsService.RecordOperation(_testPath, data.Length, false);
                return Result<byte[]>.Success(data);
            }
            catch (Exception ex)
            {
                _loggerMock.Object.LogError(ex, "Failed to read file: {FileName}", fileName);
                return Result<byte[]>.Failure(ex);
            }
        }

        private async Task<Result> WriteFileAsync(string fileName, byte[] data)
        {
            try
            {
                var fullPath = Path.Combine(_testPath, fileName);
                await File.WriteAllBytesAsync(fullPath, data);
                _metricsService.RecordOperation(_testPath, data.Length, true);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _loggerMock.Object.LogError(ex, "Failed to write file: {FileName}", fileName);
                return Result.Failure(ex);
            }
        }

        public void Dispose()
        {
            if (Directory.Exists(_testPath))
            {
                Directory.Delete(_testPath, true);
            }
        }
    }
}