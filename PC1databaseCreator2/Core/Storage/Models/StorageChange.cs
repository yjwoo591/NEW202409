using System;
using Microsoft.Extensions.Configuration;
using PC1databaseCreator.Common.Infrastructure.Base;
using PC1databaseCreator.Common.Infrastructure.Enums;  // StorageChangeType이 있는 네임스페이스

namespace PC1databaseCreator.Core.Storage.Models
{
    /// <summary>
    /// 스토리지 변경 정보
    /// </summary>
    public class StorageChange
    {
        /// <summary>
        /// 변경 ID
        /// </summary>
        public string ChangeId { get; }

        /// <summary>
        /// 변경 유형
        /// </summary>
        public StorageChangeType ChangeType { get; set; }

        /// <summary>
        /// 대상 경로
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// 새 경로 (이동 시)
        /// </summary>
        public string NewPath { get; set; }

        /// <summary>
        /// 변경 크기 (바이트)
        /// </summary>
        public long ChangeSize { get; set; }

        /// <summary>
        /// 변경 시간
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// 처리 상태
        /// </summary>
        public bool IsProcessed { get; set; }

        /// <summary>
        /// 처리 결과
        /// </summary>
        public string ProcessResult { get; set; }

        /// <summary>
        /// 오류 메시지
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public StorageChange(StorageChangeType changeType, string targetPath)
        {
            ChangeId = Guid.NewGuid().ToString();
            ChangeType = changeType;
            TargetPath = targetPath ?? throw new ArgumentNullException(nameof(targetPath));
            Timestamp = DateTime.UtcNow;
            IsProcessed = false;
        }

        /// <summary>
        /// 변경 내용 유효성 검사
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(TargetPath))
                return false;

            if (ChangeType == StorageChangeType.Move && string.IsNullOrEmpty(NewPath))
                return false;

            if (ChangeSize < 0)
                return false;

            return true;
        }

        /// <summary>
        /// 변경 처리 결과 설정
        /// </summary>
        public void SetResult(bool success, string message = null)
        {
            IsProcessed = true;
            ProcessResult = success ? "Success" : "Failed";
            ErrorMessage = success ? null : message;
        }

        /// <summary>
        /// 문자열 표현
        /// </summary>
        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {ChangeType}: {TargetPath} => {(IsProcessed ? ProcessResult : "Pending")}";
        }
    }
}