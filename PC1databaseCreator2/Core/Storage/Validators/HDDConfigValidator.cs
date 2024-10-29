using System;
using System.Linq;
using FluentValidation;
using PC1databaseCreator.Core.Storage.Models;
using PC1databaseCreator.Common.Library.Core.Storage.Models;

namespace PC1databaseCreator.Core.Storage.Validators
{
    /// <summary>
    /// HDD 설정 검증기
    /// </summary>
    public class HDDConfigValidator : AbstractValidator<HDDConfig>
    {
        public HDDConfigValidator()
        {
            RuleFor(config => config.StorageId)
                .NotEmpty().WithMessage("스토리지 ID는 필수입니다.");

            RuleFor(config => config.PrimaryDrives)
                .NotEmpty().WithMessage("주 드라이브가 하나 이상 필요합니다.");

            RuleFor(config => config.MirrorDrives)
                .NotEmpty().WithMessage("미러 드라이브가 하나 이상 필요합니다.");

            RuleFor(config => config.MirrorDrives.Count)
                .Equal(x => x.PrimaryDrives.Count)
                .WithMessage("주 드라이브와 미러 드라이브의 수가 일치해야 합니다.");

            RuleForEach(config => config.PrimaryDrives)
                .SetValidator(new StorageDriveValidator("주"));

            RuleForEach(config => config.MirrorDrives)
                .SetValidator(new StorageDriveValidator("미러"));

            RuleFor(config => config)
                .Must(HaveValidSettings)
                .WithMessage("설정값이 올바르지 않습니다.");
        }

        private bool HaveValidSettings(HDDConfig config)
        {
            var fastAccessPercentage = config.GetSetting<int>("FastAccessAreaPercentage");
            if (fastAccessPercentage < 0 || fastAccessPercentage > 100)
                return false;

            var cacheSizeMB = config.GetSetting<int>("CacheSizeMB");
            if (cacheSizeMB <= 0)
                return false;

            return true;
        }
    }

    /// <summary>
    /// 드라이브 정보 검증기
    /// </summary>
    public class StorageDriveValidator : AbstractValidator<StorageDriveInfo>
    {
        public StorageDriveValidator(string driveType)
        {
            RuleFor(drive => drive.Path)
                .NotEmpty()
                .WithMessage($"{driveType} 드라이브 경로는 필수입니다.")
                .Must(path => System.IO.Directory.Exists(path))
                .WithMessage($"{driveType} 드라이브 경로가 존재하지 않습니다: " + "{PropertyValue}");

            RuleFor(drive => drive.DriveId)
                .NotEmpty()
                .WithMessage($"{driveType} 드라이브 ID는 필수입니다.");

            RuleFor(drive => drive.Status)
                .NotEqual(DriveStatus.Error)
                .WithMessage($"{driveType} 드라이브에 오류가 있습니다.");

            RuleFor(drive => drive.IsReady)
                .Equal(true)
                .WithMessage($"{driveType} 드라이브가 준비되지 않았습니다.");

            RuleFor(drive => drive)
                .Must(drive => drive.AvailableFreeSpace > 0)
                .WithMessage($"{driveType} 드라이브의 여유 공간이 부족합니다.");
        }
    }
}