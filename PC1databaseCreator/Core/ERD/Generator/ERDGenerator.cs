using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Models.ERD;
using System.ComponentModel.DataAnnotations;

namespace PC1MAINAITradingSystem.Core.ERD.Generator
{
    public class ERDGenerator
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, IERDGenerator> _generators;

        public ERDGenerator(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _generators = InitializeGenerators();
        }

        private Dictionary<string, IERDGenerator> InitializeGenerators()
        {
            return new Dictionary<string, IERDGenerator>(StringComparer.OrdinalIgnoreCase)
            {
                { "mermaid", new MermaidGenerator(_logger) },
                { "sql", new SqlGenerator(_logger) },
                { "visual", new VisualGenerator(_logger) }
            };
        }

        public async Task<(string Content, List<string> Warnings)> GenerateERD(
            ERDModel model,
            string format = "mermaid",
            GenerationOptions options = null)
        {
            try
            {
                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                if (!_generators.TryGetValue(format.ToLower(), out var generator))
                {
                    throw new ArgumentException($"Unsupported output format: {format}");
                }

                options ??= new GenerationOptions();

                // 생성 전 유효성 검사
                if (options.ValidateBeforeGeneration)
                {
                    var validationResult = await generator.Validate(model);
                    if (!validationResult.IsValid)
                    {
                        throw new ValidationException("ERD validation failed", validationResult.Errors);
                    }
                }

                // ERD 생성
                var result = await generator.Generate(model, options);

                await _logger.LogInfo($"Generated ERD in {format} format successfully");
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Failed to generate ERD in {format} format", ex);
                throw;
            }
        }

        public async Task<List<string>> GetSupportedFormats()
        {
            return new List<string>(_generators.Keys);
        }

        public async Task<bool> IsFormatSupported(string format)
        {
            return _generators.ContainsKey(format?.ToLower() ?? string.Empty);
        }
    }

    public interface IERDGenerator
    {
        Task<ValidationResult> Validate(ERDModel model);
        Task<(string Content, List<string> Warnings)> Generate(ERDModel model, GenerationOptions options);
    }

    public abstract class BaseERDGenerator : IERDGenerator
    {
        protected readonly ILogger Logger;

        protected BaseERDGenerator(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public abstract Task<ValidationResult> Validate(ERDModel model);
        public abstract Task<(string Content, List<string> Warnings)> Generate(ERDModel model, GenerationOptions options);

        protected virtual async Task<ValidationResult> ValidateBasic(ERDModel model)
        {
            var result = new ValidationResult();

            if (model == null)
            {
                result.AddError("ERD model is null");
                return result;
            }

            if (!model.Tables.Any())
            {
                result.AddError("ERD must contain at least one table");
                return result;
            }

            foreach (var table in model.Tables)
            {
                if (string.IsNullOrWhiteSpace(table.Name))
                {
                    result.AddError("Table name cannot be empty");
                }

                if (!table.Columns.Any())
                {
                    result.AddError($"Table {table.Name} must have at least one column");
                }
            }

            return result;
        }

        protected string SanitizeIdentifier(string identifier)
        {
            return string.IsNullOrEmpty(identifier) ? string.Empty :
                new string(identifier.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        }

        protected string GetDataTypeString(ColumnModel column)
        {
            return column.DataType switch
            {
                DataType.Varchar or DataType.Nvarchar or DataType.Char or DataType.Nchar
                    when column.Length.HasValue => $"{column.DataType}({column.Length})",

                DataType.Decimal or DataType.Numeric
                    when column.Precision.HasValue && column.Scale.HasValue =>
                    $"{column.DataType}({column.Precision},{column.Scale})",

                _ => column.DataType.ToString().ToLower()
            };
        }

        protected bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            if (!char.IsLetter(name[0]) && name[0] != '_')
                return false;

            return name.All(c => char.IsLetterOrDigit(c) || c == '_');
        }
    }

    public class GenerationOptions
    {
        public bool ValidateBeforeGeneration { get; set; } = true;
        public bool IncludeMetadata { get; set; } = true;
        public bool IncludeComments { get; set; } = true;
        public bool PrettifyOutput { get; set; } = true;
        public string Indentation { get; set; } = "    ";
        public bool RemoveEmptyLines { get; set; } = false;
        public Dictionary<string, string> CustomSettings { get; set; } = new();
    }

    public class ValidationException : Exception
    {
        public List<string> ValidationErrors { get; }

        public ValidationException(string message, List<string> validationErrors)
            : base(message)
        {
            ValidationErrors = validationErrors;
        }

        public ValidationException(string message, List<string> validationErrors, Exception innerException)
            : base(message, innerException)
        {
            ValidationErrors = validationErrors;
        }
    }
}