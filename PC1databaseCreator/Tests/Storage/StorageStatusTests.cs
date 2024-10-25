using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PC1databaseCreator.Core.Storage;

namespace PC1databaseCreator.Tests.Storage
{
    [TestClass]
    public class StorageStatusTests
    {
        private HDDManager _hddManager;
        private const string TEST_DIR = "TestStorage";

        [TestInitialize]
        public void Setup()
        {
            // 테스트용 HDDConfig 설정
            var config = new HDDConfig
            {
                PrimaryDrives = new[]
                {
                    new DriveConfig { Path = $"{TEST_DIR}/Primary1", DriveNumber = 1 },
                    new DriveConfig { Path = $"{TEST_DIR}/Primary2", DriveNumber = 2 }
                },
                MirrorDrives = new[]
                {
                    new DriveConfig { Path = $"{TEST_DIR}/Mirror1", DriveNumber = 1 },
                    new DriveConfig { Path = $"{TEST_DIR}/Mirror2", DriveNumber = 2 }
                }
            };

            _hddManager = new HDDManager(config);
        }

        [TestMethod]
        public async Task SaveDataAsync_ValidData_ReturnsTrue()
        {
            // Arrange
            string testFileName = "test.dat";
            byte[] testData = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            bool result = await _hddManager.SaveDataAsync(testFileName, testData);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task LoadDataAsync_ExistingFile_ReturnsCorrectData()
        {
            // Arrange
            string testFileName = "test.dat";
            byte[] testData = new byte[] { 1, 2, 3, 4, 5 };
            await _hddManager.SaveDataAsync(testFileName, testData);

            // Act
            byte[] loadedData = await _hddManager.LoadDataAsync(testFileName);

            // Assert
            CollectionAssert.AreEqual(testData, loadedData);
        }

        [TestMethod]
        public async Task LoadDataAsync_NonExistingFile_ThrowsFileNotFoundException()
        {
            // Arrange
            string nonExistingFile = "nonexisting.dat";

            // Act & Assert
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(
                async () => await _hddManager.LoadDataAsync(nonExistingFile)
            );
        }

        [TestMethod]
        public async Task AddNewDrivePairAsync_ValidPaths_ReturnsTrue()
        {
            // Arrange
            string primaryPath = $"{TEST_DIR}/Primary3";
            string mirrorPath = $"{TEST_DIR}/Mirror3";

            // Act
            bool result = await _hddManager.AddNewDrivePairAsync(primaryPath, mirrorPath);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task SaveDataAsync_InsufficientSpace_ThrowsInvalidOperationException()
        {
            // Arrange
            string testFileName = "large.dat";
            byte[] largeData = new byte[1024 * 1024 * 1024]; // 1GB

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _hddManager.SaveDataAsync(testFileName, largeData)
            );
        }

        [TestCleanup]
        public void Cleanup()
        {
            // 테스트 디렉토리 정리
            if (Directory.Exists(TEST_DIR))
            {
                Directory.Delete(TEST_DIR, true);
            }
        }
    }
}