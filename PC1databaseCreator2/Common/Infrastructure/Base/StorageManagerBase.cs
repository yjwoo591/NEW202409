using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PC1databaseCreator.Common.Infrastructure.Interfaces;

namespace PC1databaseCreator.Common.Infrastructure.Base
{
    /// <summary>
    /// 스토리지 관리자 기본 추상 클래스
    /// </summary>
    public abstract class StorageManagerBase : IDisposable
    {
        protected readonly ILogger Logger;
        protected readonly IConfiguration Configuration;
        protected readonly string BasePath;
        private bool _disposed;

        protected StorageManagerBase(IConfiguration configuration, ILogger logger, string basePath)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            BasePath = basePath ?? throw new ArgumentNullException(nameof(basePath));

            if (!Directory.Exists(BasePath))
            {
                throw new DirectoryNotFoundException($"기본 경로가 존재하지 않습니다: {BasePath}");
            }
        }

        /// <summary>
        /// 파일 읽기
        /// </summary>
        public virtual async Task<(bool Success, byte[] Data, string Error)> ReadAsync(string path)
        {
            try
            {
                var fullPath = GetFullPath(path);
                if (!File.Exists(fullPath))
                {
                    return (false, null, $"파일을 찾을 수 없습니다: {path}");
                }

                var data = await File.ReadAllBytesAsync(fullPath);
                return (true, data, null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "파일 읽기 실패: {Path}", path);
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// 파일 쓰기
        /// </summary>
        public virtual async Task<(bool Success, string Error)> WriteAsync(string path, byte[] data)
        {
            try
            {
                if (data == null)
                {
                    return (false, "데이터가 null일 수 없습니다.");
                }

                var fullPath = GetFullPath(path);
                var directory = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllBytesAsync(fullPath, data);
                return (true, null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "파일 쓰기 실패: {Path}", path);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// 파일 삭제
        /// </summary>
        public virtual async Task<(bool Success, string Error)> DeleteAsync(string path)
        {
            try
            {
                var fullPath = GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    return (true, null);
                }
                return (false, $"파일을 찾을 수 없습니다: {path}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "파일 삭제 실패: {Path}", path);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public abstract Task InitializeAsync();

        /// <summary>
        /// 전체 경로 가져오기
        /// </summary>
        protected virtual string GetFullPath(string relativePath)
        {
            return Path.Combine(BasePath, relativePath.TrimStart('/'));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 관리되는 리소스 해제
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}