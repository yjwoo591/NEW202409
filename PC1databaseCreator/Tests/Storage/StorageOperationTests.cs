using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PC1databaseCreator.Core.Storage;
using PC1databaseCreator.Core.Storage.Common;
using PC1databaseCreator.Core.Storage.Interfaces;

namespace PC1databaseCreator.Tests.Storage
{
    [TestClass]
    public class StorageOperationTests
    {
        private StorageOperation _storageOperation;
        private string _testPath;
        private string _sourcePath;
        private string _targetPath;

        [TestInitialize]
        public void Setup()
        {
            _testPath = Path.Combine(Path.GetTempPath(), "StorageOperationTests");
            _sourcePath = Path.Combine(_testPath, "source");
            _targetPath = Path.Combine(_testPath, "target");

            Directory.CreateDirectory(_testPath);
            Directory.CreateDirectory(_sourcePath);
            Directory.CreateDirectory(_targetPath);

            _storageOperation = new StorageOperation(_testPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (Directory.Exists(_testPath))
                {
                    Directory.Delete(_testPath, true);
                }
            }
            catch
            {
                // 테스트 정리 중 오류는 무시
            }
        }

        [TestMethod]
        public async Task CopyFileAsync_ValidPaths_Success()
        {
            // Arrange
            var sourceFile = Path.Combine(_sourcePath, "test.txt");
            var targetFile = Path.Combine(_targetPath, "test.txt");
            await File.WriteAllTextAsync(sourceFile, "Test content");

            // Act
            var result = await _storageOperation.CopyFileAsync(sourceFile, targetFile);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(File.Exists(targetFile));
            Assert.AreEqual("Test content", await File.ReadAllTextAsync(targetFile));
            Assert.IsTrue(result.ProcessedBytes > 0);
        }

        [TestMethod]
        public async Task CopyFileAsync_SourceNotExists_Fails()
        {
            // Arrange
            var sourceFile = Path.Combine(_sourcePath, "nonexistent.txt");
            var targetFile = Path.Combine(_targetPath, "test.txt");

            // Act
            var result = await _storageOperation.CopyFileAsync(sourceFile, targetFile);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsFalse(File.Exists(targetFile));
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
        }

        [TestMethod]
        public async Task MoveFileAsync_ValidPaths_Success()
        {
            // Arrange
            var sourceFile = Path.Combine(_sourcePath, "test.txt");
            var targetFile = Path.Combine(_targetPath, "test.txt");
            await File.WriteAllTextAsync(sourceFile, "Test content");

            // Act
            var result = await _storageOperation.MoveFileAsync(sourceFile, targetFile);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(File.Exists(targetFile));
            Assert.IsFalse(File.Exists(sourceFile));
            Assert.IsTrue(result.ProcessedBytes > 0);
        }

        [TestMethod]
        public async Task CreateDirectoryAsync_ValidPath_Success()
        {
            // Arrange
            var newPath = Path.Combine(_testPath, "newdir");

            // Act
            var result = await _storageOperation.CreateDirectoryAsync(newPath);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(Directory.Exists(newPath));
        }

        [TestMethod]
        public async Task DeleteAsync_ValidFile_Success()
        {
            // Arrange
            var testFile = Path.Combine(_sourcePath, "test.txt");
            await File.WriteAllTextAsync(testFile, "Test content");

            // Act
            var result = await _storageOperation.DeleteAsync(testFile);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsFalse(File.Exists(testFile));
        }

        [TestMethod]
        public async Task DeleteAsync_ValidDirectory_Success()
        {
            // Arrange
            var testDir = Path.Combine(_testPath, "testdir");
            Directory.CreateDirectory(testDir);

            // Act
            var result = await _storageOperation.DeleteAsync(testDir, true);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsFalse(Directory.Exists(testDir));
        }

        [TestMethod]
        public async Task CalculateDirectorySizeAsync_ValidDirectory_ReturnsCorrectSize()
        {
            // Arrange
            var testDir = Path.Combine(_testPath, "sizetest");
            Directory.CreateDirectory(testDir);

            var content = new string('x', 1000); // 1000 bytes content
            await File.WriteAllTextAsync(Path.Combine(testDir, "file1.txt"), content);
            await File.WriteAllTextAsync(Path.Combine(testDir, "file2.txt"), content);

            // Act
            var size = await _storageOperation.CalculateDirectorySizeAsync(testDir);

            // Assert
            Assert.AreEqual(2000, size); // 2 files * 1000 bytes
        }

        [TestMethod]
        public void HasEnoughSpace_ValidDrive_ReturnsExpectedResult()
        {
            // Arrange
            var drivePath = Path.GetPathRoot(_testPath);

            // Act
            var result = _storageOperation.HasEnoughSpace(drivePath, 1024); // 1KB

            // Assert
            Assert.IsTrue(result); // Assuming test machine has more than 1KB + MIN_FREE_SPACE_GB
        }

        [TestMethod]
        public async Task CopyFileAsync_LargeFile_CorrectProgress()
        {
            // Arrange
            var sourceFile = Path.Combine(_sourcePath, "large.txt");
            var targetFile = Path.Combine(_targetPath, "large.txt");
            var content = new string('x', 1024 * 1024); // 1MB content
            await File.WriteAllTextAsync(sourceFile, content);

            // Act
            var result = await _storageOperation.CopyFileAsync(sourceFile, targetFile);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1024 * 1024, result.ProcessedBytes);
            Assert.IsTrue(File.Exists(targetFile));
        }

        [TestMethod]
        public async Task CopyFileAsync_WithPriority_Success()
        {
            // Arrange
            var sourceFile = Path.Combine(_sourcePath, "priority.txt");
            var targetFile = Path.Combine(_targetPath, "priority.txt");
            await File.WriteAllTextAsync(sourceFile, "Test content");

            // Act
            var result = await _storageOperation.CopyFileAsync(sourceFile, targetFile, OperationPriority.High);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(OperationPriority.High, result.Priority);
        }
    }
}