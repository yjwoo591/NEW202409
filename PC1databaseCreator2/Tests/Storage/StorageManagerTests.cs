using Xunit;
using FluentAssertions;
using PC1databaseCreator.Core.Storage;
using PC1databaseCreator.Common.Results;
using PC1databaseCreator.Common.Infrastructure.Interfaces.Logging;
using Moq;

namespace PC1databaseCreator.Tests.Storage
{
    public class StorageManagerTests
    {
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly string _testPath;

        public StorageManagerTests()
        {
            _loggerMock = new Mock<ILoggerService>();
            _testPath = Path.Combine(Path.GetTempPath(), "StorageTests");

            // 테스트 디렉토리 초기화
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

            var storageManager = CreateStorageManager();

            // Act
            var result = await storageManager.WriteAsync(fileName, testData);

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

            var storageManager = CreateStorageManager();

            // Act
            var result = await storageManager.ReadAsync(fileName);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(testData);
        }

        [Fact]
        public async Task ReadFile_NonExistingFile_ShouldReturnFailure()
        {
            // Arrange
            var fileName = "nonexistent.dat";
            var storageManager = CreateStorageManager();

            // Act
            var result = await storageManager.ReadAsync(fileName);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("not found") || m.Contains("존재하지 않습니다"));
        }

        [Fact]
        public async Task DeleteFile_ExistingFile_ShouldSucceed()
        {
            // Arrange
            var fileName = "test.dat";
            var fullPath = Path.Combine(_testPath, fileName);
            await File.WriteAllBytesAsync(fullPath, new byte[] { 1, 2, 3 });

            var storageManager = CreateStorageManager();

            // Act
            var result = await storageManager.DeleteAsync(fileName);

            // Assert
            result.IsSuccess.Should().BeTrue();
            File.Exists(fullPath).Should().BeFalse();
        }

        [Fact]
        public async Task WriteFile_InvalidPath_ShouldReturnFailure()
        {
            // Arrange
            var testData = new byte[] { 1, 2, 3, 4, 5 };
            var invalidFileName = "invalid/path/test.dat";

            var storageManager = CreateStorageManager();

            // Act
            var result = await storageManager.WriteAsync(invalidFileName, testData);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain(m => m.Contains("path") || m.Contains("경로"));
        }

        private TestStorageManager CreateStorageManager()
        {
            return new TestStorageManager(_testPath, _loggerMock.Object);
        }
    }

    // 테스트를 위한 구체 클래스
    public class TestStorageManager : StorageManager
    {
        public TestStorageManager(string basePath, ILoggerService logger)
            : base(basePath, logger)
        {
        }

        // 추가 테스트 메서드가 필요한 경우 여기에 구현
    }
}