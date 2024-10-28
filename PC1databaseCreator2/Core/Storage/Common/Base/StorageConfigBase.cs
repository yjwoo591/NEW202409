using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using PC1databaseCreator.Core.Storage.Common.Enums;

namespace PC1databaseCreator.Core.Storage.Common.Base
{
    /// <summary>
    /// 스토리지 설정 기본 추상 클래스
    /// </summary>
    public abstract class StorageConfigBase : IDisposable
    {
        /// <summary>
        /// 설정 인스턴스
        /// </summary>
        protected readonly IConfiguration Configuration;

        /// <summary>
        /// 설정값 저장소
        /// </summary>
        protected readonly Dictionary<string, object> Settings;

        /// <summary>
        /// 스토리지 ID
        /// </summary>
        public string StorageId { get; protected set; }

        /// <summary>
        /// 스토리지 유형
        /// </summary>
        public StorageType Type { get; protected set; }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="configuration">설정 인스턴스</param>
        /// <param name="storageType">스토리지 유형</param>
        protected StorageConfigBase(IConfiguration configuration, StorageType storageType)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Type = storageType;
            Settings = new Dictionary<string, object>();
        }

        /// <summary>
        /// 설정값 가져오기
        /// </summary>
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

        /// <summary>
        /// 설정값 저장
        /// </summary>
        public void SetSetting<T>(string key, T value)
        {
            Settings[key] = value;
            OnSettingsChanged();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 설정 유효성 검사
        /// </summary>
        public abstract bool Validate();

        /// <summary>
        /// 설정 변경 이벤트
        /// </summary>
        protected virtual void OnSettingsChanged()
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 설정 변경 이벤트 핸들러
        /// </summary>
        public event EventHandler SettingsChanged;

        #region IDisposable Support
        private bool disposedValue = false;

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
        #endregion
    }
}