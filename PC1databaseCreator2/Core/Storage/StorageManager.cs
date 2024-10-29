using Microsoft.Extensions.Logging;
using PC1databaseCreator.Common.Infrastructure.Interfaces.Logging;
using PC1databaseCreator.Common.Results;
using System.Collections.Concurrent;

namespace PC1databaseCreator.Core.Storage
{
    public abstract class StorageManager
    {
        protected readonly string BasePath;
        protected readonly ILoggerService Logger;
        protected readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks;

        protected StorageManager(string basePath, ILoggerService logger)
        {
            if (string.IsNullOrWhiteSpace(basePath))
                throw new ArgumentNullException(nameof(basePath));

            BasePath = basePath;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            FileLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

            // 기본 경로 생성
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
        }

        public virtual async Task<Result<byte[]>> ReadAsync(string fileName)
        {
            var fullPath = Path.Combine(BasePath, fileName);
            var fileLock = GetFileLock(fullPath);

            try
            {
                await fileLock.WaitAsync();

                if (!File.Exists(fullPath))
                {
                    return Result<byte[]>.Failure($"File not found: {fileName}");
                }

                var data = await File.ReadAllBytesAsync(fullPath);
                return Result<byte[]>.Success(data);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to read file: {FileName}", fileName);
                return Result<byte[]>.Failure(ex);
            }
            finally
            {
                fileLock.Release();
            }
        }

        public virtual async Task<Result> WriteAsync(string fileName, byte[] data)
        {
            if (data == null)
                return Result.Failure("Data cannot be null");

            var fullPath = Path.Combine(BasePath, fileName);
            var fileLock = GetFileLock(fullPath);

            try
            {
                await fileLock.WaitAsync();

                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllBytesAsync(fullPath, data);
                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to write file: {FileName}", fileName);
                return Result.Failure(ex);
            }
            finally
            {
                fileLock.Release();
            }
        }

        public virtual async Task<Result> DeleteAsync(string fileName)
        {
            var fullPath = Path.Combine(BasePath, fileName);
            var fileLock = GetFileLock(fullPath);

            try
            {
                await fileLock.WaitAsync();

                if (!File.Exists(fullPath))
                {
                    return Result.Success();
                }

                File.Delete(fullPath);
                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to delete file: {FileName}", fileName);
                return Result.Failure(ex);
            }
            finally
            {
                fileLock.Release();
            }
        }

        protected SemaphoreSlim GetFileLock(string path)
        {
            return FileLocks.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var fileLock in FileLocks.Values)
                {
                    fileLock.Dispose();
                }
            }
        }
    }
}