using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PC1databaseCreator.Common.Library.Core.Storage;
using PC1databaseCreator.Common.Library.Core.Storage.Models;
using PC1databaseCreator.Core.Storage.Models;

namespace PC1databaseCreator.Tests.Storage
{
    [TestClass]
    public class StorageStatusTests
    {
        private Mock<ILogger<HDDConfig>> _loggerMock;
        private Mock<IConfiguration> _configMock;
        private MockFileSystem _mockFileSystem;
        private string _testBasePath;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<HDDConfig>>();
            _configMock = new Mock<IConfiguration>();
            _mockFileSystem = new MockFileSystem();
            _testBasePath = @"C:\TestStorage";
        }

        [TestMethod]
        public void HDDConfig_Initialize_CreatesRequiredDirectories()
        {
            // Arrange
            var primaryPath = _mockFileSystem.Path.Combine(_testBasePath, "Primary");
            var mirrorPath = _mockFileSystem.Path.Combine(_testBasePath, "Mirror");
            var config = new HDDConfig(_configMock.Object, _mockFileSystem, _loggerMock.Object);

            // Act
            config.AddDrivePair(primaryPath, mirrorPath);
            config.Initialize();

            // Assert
            Assert.IsTrue(_mockFileSystem.Directory.Exists(
                _mockFileSystem.Path.Combine(primaryPath, "FastAccess")));
            Assert.IsTrue(_mockFileSystem.Directory.Exists(
                _mockFileSystem.Path.Combine(primaryPath, "Archive")));
            Assert.IsTrue(_mockFileSystem.Directory.Exists(
                _mockFileSystem.Path.Combine(mirrorPath, "FastAccess")));
            Assert.IsTrue(_mockFileSystem.Directory.Exists(
                _mockFileSystem.Path.Combine(mirrorPath, "Archive")));
        }

        [TestMethod]
        public void HDDConfig_AddDrivePair_AddsValidPaths()
        {
            // Arrange
            var config = new HDDConfig(_configMock.Object, _mockFileSystem, _loggerMock.Object);
            var primaryPath = _mockFileSystem.Path.Combine(_testBasePath, "Primary");
            var mirrorPath = _mockFileSystem.Path.Combine(_testBasePath, "Mirror");

            // Create test directories
            _mockFileSystem.Directory.CreateDirectory(primaryPath);
            _mockFileSystem.Directory.CreateDirectory(mirrorPath);

            // Act
            var result = config.AddDrivePair(primaryPath, mirrorPath);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, config.PrimaryDrives.Count);
            Assert.AreEqual(1, config.MirrorDrives.Count);
            Assert.AreEqual(primaryPath, config.PrimaryDrives[0]);
            Assert.AreEqual(mirrorPath, config.MirrorDrives[0]);
        }

        [TestMethod]
        public void HDDConfig_AddDrivePair_RejectsInvalidPaths()
        {
            // Arrange
            var config = new HDDConfig(_configMock.Object, _mockFileSystem, _loggerMock.Object);

            // Act
            var result = config.AddDrivePair(string.Empty, _testBasePath);

            // Assert
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(0, config.PrimaryDrives.Count);
            Assert.AreEqual(0, config.MirrorDrives.Count);
        }

        [TestMethod]
        public void HDDConfig_Validate_ReturnsTrueForValidConfig()
        {
            // Arrange
            var config = new HDDConfig(_configMock.Object, _mockFileSystem, _loggerMock.Object);
            var primaryPath = _mockFileSystem.Path.Combine(_testBasePath, "Primary");
            var mirrorPath = _mockFileSystem.Path.Combine(_testBasePath, "Mirror");

            // Create test directories
            _mockFileSystem.Directory.CreateDirectory(primaryPath);
            _mockFileSystem.Directory.CreateDirectory(mirrorPath);

            config.AddDrivePair(primaryPath, mirrorPath);

            // Act
            var isValid = config.Validate();

            // Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void HDDConfig_Validate_ReturnsFalseForInvalidConfig()
        {
            // Arrange
            var config = new HDDConfig(_configMock.Object, _mockFileSystem, _loggerMock.Object);

            // Don't add any drive pairs - this should make the config invalid

            // Act
            var isValid = config.Validate();

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up any test directories if needed
            if (_mockFileSystem.Directory.Exists(_testBasePath))
            {
                _mockFileSystem.Directory.Delete(_testBasePath, true);
            }
        }
    }
}