using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PC1databaseCreator.Core.Storage.Infrastructure;
using PC1databaseCreator.Core.Storage.Models;

namespace PC1databaseCreator.Tests.Storage.Infrastructure
{
    [TestClass]
    public class FileSystemProviderTests
    {
        private MockFileSystem _mockFileSystem;
        private Mock<ILogger<FileSystemProvider>> _loggerMock;
        private FileSystemProvider _provider;
        private string _rootPath;
        private const string TEST_DIRECTORY = "TestStorage";

        [TestInitialize]
        public void Setup()
        {
            _rootPath = Path.Combine(Path.GetTempPath(), TEST_DIRECTORY);
            _mockFileSystem = new MockFileSystem();
            _loggerMock = new Mock<ILogger<FileSystemProvider>>();

            // 기본 디렉토리 구조 설정
            var fileSystemSetup = new Dictionary<string, MockFileData>
            {
                { Path.Combine(_rootPath, "FastAccess"), new MockDirectoryData() },
                { Path.Combine(_rootPath, "Archive"), new MockDirectoryData() }
            };

            _mockFileSystem = new MockFileSystem(fileSystemSetup);

            // 테스트용 드라이브 설정
            var mockDriveInfo = new MockDriveInfo(
                _mockFileSystem,
                new DriveInfoFactory(),
                "C:",
                1024L * 1024L * 1024L * 100L, // 100GB 총 용량
                1024L * 1024L * 1024L * 50L    // 50GB 가용 용량
            );
            _mockFileSystem.AddDrive(mockDriveInfo);

            _provider = new FileSystemProvider(_mockFileSystem, _loggerMock.Object, _rootPath);
        }

        [TestMethod]
        public async Task ReadFileAsync_ExistingFile_ReturnsCorrectData()
        {
            // Arrange
            var filePath = "test.txt";
            var expectedData = new byte[] { 1, 2, 3, 4, 5 };
            var fullPath = Path.Combine(_rootPath, filePath);
            _mockFileSystem.AddFile(fullPath, new MockFileData(expectedData));

            // Act
            var result = await _provider.ReadFileAsync(filePath, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            CollectionAssert.AreEqual(expectedData, result.Data);
        }

        [TestMethod]
        public async Task ReadFileAsync_NonExistentFile_ReturnsFailure()
        {
            // Arrange
            var filePath = "nonexistent.txt";

            // Act
            var result = await _provider.ReadFileAsync(filePath, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(StorageErrorType.PathNotFound, result.ErrorType);
        }

        [TestMethod]
        public async Task WriteFileAsync_ValidData_SavesSuccessfully()
        {
            // Arrange
            var filePath = "newfile.txt";
            var data = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            var result = await _provider.WriteFileAsync(filePath, data, CancellationToken.None);
            var fullPath = Path.Combine(_rootPath, filePath);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_mockFileSystem.File.Exists(fullPath));
            var savedData = _mockFileSystem.File.ReadAllBytes(fullPath);
            CollectionAssert.AreEqual(data, savedData);
        }

        [TestMethod]
        public async Task WriteFileAsync_InsufficientSpace_ReturnsFailure()
        {
            // Arrange
            var filePath = "largefile.txt";
            var data = new byte[1024L * 1024L * 1024L * 60L]; // 60GB (더 큰 용량)

            // Act
            var result = await _provider.WriteFileAsync(filePath, data, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(StorageErrorType.InsufficientSpace, result.ErrorType);
        }

        [TestMethod]
        public void DeleteFile_ExistingFile_DeletesSuccessfully()
        {
            // Arrange
            var filePath = "todelete.txt";
            var fullPath = Path.Combine(_rootPath, filePath);
            _mockFileSystem.AddFile(fullPath, new MockFileData(new byte[] { 1, 2, 3 }));

            // Act
            var result = _provider.DeleteFile(filePath);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(_mockFileSystem.File.Exists(fullPath));
        }

        [TestMethod]
        public void DeleteFile_NonExistentFile_ReturnsSuccess()
        {
            // Arrange
            var filePath = "nonexistent.txt";

            // Act
            var result = _provider.DeleteFile(filePath);

            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void GetMetrics_ReturnsCorrectValues()
        {
            // Act
            var result = _provider.GetMetrics();

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(100L * 1024L * 1024L * 1024L, result.Data.TotalSpace);
            Assert.AreEqual(50L * 1024L * 1024L * 1024L, result.Data.AvailableSpace);
            Assert.AreEqual(50L * 1024L * 1024L * 1024L, result.Data.UsedSpace);
            Assert.AreEqual(50.0, result.Data.UsagePercentage);
        }

        [TestMethod]
        public void GetMetrics_HighUsage_ReturnsCriticalStatus()
        {
            // Arrange
            var mockDriveInfo = new MockDriveInfo(
                _mockFileSystem,
                new DriveInfoFactory(),
                "C:",
                1024L * 1024L * 1024L * 100L, // 100GB 총 용량
                1024L * 1024L * 1024L * 5L     // 5GB 가용 용량 (95% 사용)
            );
            _mockFileSystem.AddDrive(mockDriveInfo);

            // Act
            var result = _provider.GetMetrics();

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(StorageStatus.Critical, result.Data.Status);
        }

        [TestMethod]
        public async Task WriteFileAsync_CreatesIntermediateDirectories()
        {
            // Arrange
            var filePath = Path.Combine("subfolder1", "subfolder2", "newfile.txt");
            var data = new byte[] { 1, 2, 3 };

            // Act
            var result = await _provider.WriteFileAsync(filePath, data, CancellationToken.None);
            var fullPath = Path.Combine(_rootPath, filePath);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_mockFileSystem.File.Exists(fullPath));
            Assert.IsTrue(_mockFileSystem.Directory.Exists(Path.GetDirectoryName(fullPath)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullFileSystem_ThrowsArgumentNullException()
        {
            // Act
            new FileSystemProvider(null, _loggerMock.Object, _rootPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Act
            new FileSystemProvider(_mockFileSystem, null, _rootPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullRootPath_ThrowsArgumentNullException()
        {
            // Act
            new FileSystemProvider(_mockFileSystem, _loggerMock.Object, null);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // 실제 파일 시스템에 생성된 테스트 디렉토리가 있다면 제거
            if (Directory.Exists(_rootPath))
            {
                Directory.Delete(_rootPath, true);
            }
        }
    }
}