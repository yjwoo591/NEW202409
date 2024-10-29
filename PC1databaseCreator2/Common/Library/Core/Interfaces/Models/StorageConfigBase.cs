using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using PC1databaseCreator.Common.Library.Core.Interfaces;

namespace PC1databaseCreator.Common.Library.Core.Models
{
    /// <summary>
    /// 스토리지 설정 기본 클래스
    /// </summary>
    public abstract class StorageConfigBase : IStorageConfig
    {
        private bool _disposed;
        protected readonly IConfiguration Configuration;
        protected readonly Dictionary<string, object> Settings;

        public string StorageId { get; protected set; }
        public StorageType StorageType { get; protected set; }

        protected StorageConfigBase(IConfiguration configuration, StorageType storageType)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            StorageType = storageType;
            Settings = new Dictionary<string, object>();
        }

        public T GetSetting<T>(string key, T defaultValue = default)
        {
            if (Settings.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public void SetSetting<T>(string key, T value)
        {
            Settings[key] = value;
            OnSettingsChanged();
        }

        public abstract bool Validate();

        public abstract void Initialize();

        protected virtual void OnSettingsChanged()
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 관리되는 리소스 해제
                    Settings.Clear();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event EventHandler SettingsChanged;
    }
}