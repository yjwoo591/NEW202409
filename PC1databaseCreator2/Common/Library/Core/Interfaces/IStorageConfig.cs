using System;

namespace PC1databaseCreator.Common.Library.Core.Interfaces
{
    /// <summary>
    /// 스토리지 설정의 기본 인터페이스
    /// </summary>
    public interface IStorageConfig : IDisposable
    {
        /// <summary>
        /// 스토리지 ID
        /// </summary>
        string StorageId { get; }

        /// <summary>
        /// 스토리지 유형
        /// </summary>
        StorageType StorageType { get; }

        /// <summary>
        /// 설정값 가져오기
        /// </summary>
        T GetSetting<T>(string key, T defaultValue = default);

        /// <summary>
        /// 설정값 저장
        /// </summary>
        void SetSetting<T>(string key, T value);

        /// <summary>
        /// 설정 검증
        /// </summary>
        bool Validate();

        /// <summary>
        /// 설정 초기화
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// 스토리지 유형
    /// </summary>
    public enum StorageType
    {
        SSD1,   // 실시간 데이터용 SSD
        SSD2,   // 백업용 SSD
        HDD     // 아카이브용 HDD
    }
}