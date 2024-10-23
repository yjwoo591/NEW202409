using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Models.ERD;
using System.ComponentModel.DataAnnotations;

namespace PC1MAINAITradingSystem.Core.ERD.Parser
{
    public class ERDParser
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, IERDParser> _parsers;

        public ERDParser(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parsers = InitializeParsers();
        }

        public async Task<(ERDModel Model, List<string> Errors)> ParseERD(string content, string format = "mermaid")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return (null, new List<string> { "ERD content is empty" });
                }

                if (!_parsers.TryGetValue(format.ToLower(), out var parser))
                {
                    return (null, new List<string> { $"Unsupported ERD format: {format}" });
                }

                // 기본 구문 검사
                var syntaxErrors = await parser.ValidateSyntax(content);
                if (syntaxErrors.Count > 0)
                {
                    return (null, syntaxErrors);
                }

                // ERD 파싱 수행
                var model = await parser.Parse(content);

                await _logger.LogInfo($"ERD parsing completed successfully using {format} parser");
                return (model, new List<string>());
            }
            catch (Exception ex)
            {
                await _logger.LogError($"ERD parsing failed: {ex.Message}", ex);
                return (null, new List<string> { ex.Message });
            }
        }

        private Dictionary<string, IERDParser> InitializeParsers()
        {
            return new Dictionary<string, IERDParser>(StringComparer.OrdinalIgnoreCase)
            {
                { "mermaid", new MermaidParser(_logger) },
                // 다른 파서들을 여기에 추가
            };
        }

        public async Task<List<string>> GetSupportedFormats()
        {
            return new List<string>(_parsers.Keys);
        }

        public async Task<bool> IsFormatSupported(string format)
        {
            return _parsers.ContainsKey(format?.ToLower() ?? string.Empty);
        }
    }

    public interface IERDParser
    {
        Task<List<string>> ValidateSyntax(string content);
        Task<ERDModel> Parse(string content);
        Task<string> Generate(ERDModel model);
    }

    public abstract class BaseERDParser : IERDParser
    {
        protected readonly ILogger Logger;

        protected BaseERDParser(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public abstract Task<List<string>> ValidateSyntax(string content);
        public abstract Task<ERDModel> Parse(string content);
        public abstract Task<string> Generate(ERDModel model);

        protected async Task<bool> ValidateBasicSyntax(string content, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                errors.Add("Content cannot be empty");
                return false;
            }

            return true;
        }

        protected DataType ParseDataType(string typeString)
        {
            return typeString.ToLower() switch
            {
                "int" => DataType.Int,
                "bigint" => DataType.Bigint,
                "varchar" => DataType.Varchar,
                "nvarchar" => DataType.Nvarchar,
                "char" => DataType.Char,
                "nchar" => DataType.Nchar,
                "decimal" => DataType.Decimal,
                "numeric" => DataType.Numeric,
                "datetime" => DataType.Datetime,
                "bool" or "boolean" => DataType.Bool,
                "binary" => DataType.Binary,
                "varbinary" => DataType.Varbinary,
                _ => throw new ArgumentException($"Unsupported data type: {typeString}")
            };
        }

        protected (int? Length, int? Precision, int? Scale) ParseTypeParameters(string typeString)
        {
            // Example formats: varchar(50), decimal(18,2)
            try
            {
                var startIndex = typeString.IndexOf('(');
                if (startIndex == -1)
                {
                    return (null, null, null);
                }

                var endIndex = typeString.IndexOf(')');
                if (endIndex == -1)
                {
                    throw new FormatException($"Invalid type parameter format: {typeString}");
                }

                var parameters = typeString.Substring(startIndex + 1, endIndex - startIndex - 1)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries);

                switch (parameters.Length)
                {
                    case 1:
                        return (int.Parse(parameters[0]), null, null);
                    case 2:
                        return (null, int.Parse(parameters[0]), int.Parse(parameters[1]));
                    default:
                        throw new FormatException($"Invalid number of type parameters: {typeString}");
                }
            }
            catch (Exception ex)
            {
                throw new FormatException($"Failed to parse type parameters: {typeString}", ex);
            }
        }

        protected string GetBaseType(string fullType)
        {
            var index = fullType.IndexOf('(');
            return index == -1 ? fullType : fullType.Substring(0, index);
        }

        protected bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            if (!char.IsLetter(name[0]) && name[0] != '_')
                return false;

            return name.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        protected string NormalizeIdentifier(string identifier)
        {
            return identifier?.Trim().Replace(" ", "_") ?? string.Empty;
        }
    }
}