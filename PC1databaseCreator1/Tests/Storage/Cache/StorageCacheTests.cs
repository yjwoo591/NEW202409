using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using PC1databaseCreator.Core.Storage.Cache;
using PC1databaseCreator.Core.Storage.Models;

namespace PC1databaseCreator.Tests.Storage.Cache
{
    [TestClass]
    public class StorageCacheTests
    {
        private StorageCache _cache;
        private IMemoryCache _memoryCache;
        private Mock<ILogger<StorageCache>> _loggerMock;

        [TestInitialize]
        public void Setup()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _loggerMock = new Mock<ILogger<StorageCache>>();
            _cache = new StorageCache(_memoryCache, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cache.Dispose();
            _memoryCache.Dispose();
        }

        [TestMethod]
        public async Task GetAsync_NonExistentKey_ReturnsNullSuccessfully()
        {
            // Arrange
            var key = "nonexistent";

            // Act
            var result = await _cache.GetAsync(key);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Data);
        }

        [TestMethod]
        public async Task SetAndGetAsync_ValidData_WorksCorrectly()
        {
            // Arrange
            var key = "test-key";
            var data = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            var setResult = await _cache.SetAsync(key, data);
            var getResult = await _cache.GetAsync(key);

            // Assert
            Assert.IsTrue(setResult.IsSuccess);
            Assert.IsTrue(getResult.IsSuccess);
            CollectionAssert.AreEqual(data, getResult.Data);
        }

        [TestMethod]
        public async Task RemoveAsync_ExistingKey_RemovesSuccessfully()
        {
            // Arrange
            var key = "test-key";
            var data = new byte[] { 1, 2, 3, 4, 5 };
            await _cache.SetAsync(key, data);

            // Act
            var removeResult = await _cache.RemoveAsync(key);
            var getResult = await _cache.GetAsync(key);

            // Assert
            Assert.IsTrue(removeResult.IsSuccess);
            Assert.IsTrue(getResult.IsSuccess);
            Assert.IsNull(getResult.Data);
        }

        [TestMethod]
        public async Task SetAsync_NullKey_ReturnsFailure()
        {
            // Arrange
            string key = null;
            var data = new byte[] { 1, 2, 3 };

            // Act
            var result = await _cache.SetAsync(key, data);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(StorageErrorType.InvalidOperation, result.ErrorType);
        }

        [TestMethod]
        public async Task SetAsync_NullData_ReturnsFailure()
        {
            // Arrange
            var key = "test-key";
            byte[] data = null;

            // Act
            var result = await _cache.SetAsync(key, data);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(StorageErrorType.InvalidOperation, result.ErrorType);
        }

        [TestMethod]
        public async Task ClearAsync_ClearsAllData()
        {
            // Arrange
            var keys = new[] { "key1", "key2", "key3" };
            var data = new byte[] { 1, 2, 3 };
            foreach (var key in keys)
            {
                await _cache.SetAsync(key, data);
            }

            // Act
            var clearResult = await _cache.ClearAsync();
            var getAllResults = await Task.WhenAll(
                keys.Select(k => _cache.GetAsync(k)));

            // Assert
            Assert.IsTrue(clearResult.IsSuccess);
            Assert.IsTrue(getAllResults.All(r => r.Data == null));
        }

        [TestMethod]
        public async Task SetPolicyAsync_ValidPolicy_UpdatesSuccessfully()
        {
            // Arrange
            var policy = new CachePolicy
            {
                DefaultExpiration = TimeSpan.FromMinutes(60),
                EnableCompression = false
            };

            // Act
            var result = await _cache.SetPolicyAsync(policy);

            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public async Task GetCacheSizeAsync_ReturnsCorrectSize()
        {
            // Arrange
            var key = "test-key";
            var data = new byte[1024]; // 1KB of data
            await _cache.SetAsync(key, data);

            // Act
            var result = await _cache.GetCacheSizeAsync();

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data > 0);
        }

        [TestMethod]
        public async Task GetItemCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var keys = new[] { "key1", "key2", "key3" };
            var data = new byte[] { 1, 2, 3 };
            foreach (var key in keys)
            {
                await _cache.SetAsync(key, data);
            }

            // Act
            var result = await _cache.GetItemCountAsync();

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, result.Data);
        }
    }
}