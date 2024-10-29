﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using PC1databaseCreator.Common.Infrastructure.Enums;

namespace PC1databaseCreator.Common.Infrastructure.Base
{
    /// <summary>
    /// 스토리지 설정 기본 추상 클래스
    /// </summary>
    public abstract class StorageConfigBase : IDisposable
    {
        protected readonly IConfiguration Configuration;
        protected readonly Dictionary<string, object> Settings;

        public string StorageId { get; protected set; }
        public StorageType Type { get; protected set; }

        protected StorageConfigBase(IConfiguration configuration, StorageType storageType)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Type = storageType;
            Settings = new Dictionary<string, object>();
        }

        public T GetSetting<T>(string key, T defaultValue = default)
        {
            if (Settings.TryGetValue(key, out object value))
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

        public abstract void Initialize();

        public abstract bool Validate();

        protected virtual void OnSettingsChanged()
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SettingsChanged;

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Settings.Clear();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}