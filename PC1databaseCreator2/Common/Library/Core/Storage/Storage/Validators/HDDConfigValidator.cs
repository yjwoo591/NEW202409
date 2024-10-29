using System.IO.Abstractions;
using FluentValidation;
using PC1databaseCreator.Common.Library.Core.Storage;

namespace PC1databaseCreator.Common.Library.Core.Storage
{
    /// <summary>
    /// HDDConfig 유효성 검사기
    /// </summary>
    public class HDDConfigValidator : AbstractValidator<HDDConfig>
    {
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// HDDConfigValidator 생성자
        /// </summary>
        public HDDConfigValidator(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;

            RuleFor(x => x.PrimaryDrives)
                .NotEmpty().WithMessage("Primary drives list cannot be empty");

            RuleFor(x => x.MirrorDrives)
                .NotEmpty().WithMessage("Mirror drives list cannot be empty")
                .Must((config, mirrors) => mirrors.Count == config.PrimaryDrives.Count)
                .WithMessage("Number of primary and mirror drives must match");

            RuleForEach(x => x.PrimaryDrives)
                .Must(path => !string.IsNullOrEmpty(path) && _fileSystem.Directory.Exists(path))
                .WithMessage("Primary drive path must exist");

            RuleForEach(x => x.MirrorDrives)
                .Must(path => !string.IsNullOrEmpty(path) && _fileSystem.Directory.Exists(path))
                .WithMessage("Mirror drive path must exist");
        }
    }
}