using System;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Core.Parser;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private async Task<bool> ValidateERDContent()
        {
            try
            {
                var validationForm = new ValidationForm();
                string content = GetERDContent();

                // 기본 구문 검사
                if (!await _erdParser.ValidateSyntax(content))
                {
                    validationForm.AddValidationResult("Error", "구문", "ERD 구문이 올바르지 않습니다.");
                    validationForm.ShowDialog();
                    return false;
                }

                // ERD 모델 유효성 검사
                var validationResults = await ValidateERDModel(_currentERD);
                bool hasErrors = false;

                foreach (var result in validationResults)
                {
                    validationForm.AddValidationResult(
                        result.Severity,
                        result.Location,
                        result.Message
                    );
                    if (result.Severity == "Error") hasErrors = true;
                }

                if (validationForm.HasResults)
                {
                    validationForm.ShowDialog();
                }

                await _logger.Info($"ERD validation completed with {validationResults.Count} findings");
                return !hasErrors;
            }
            catch (Exception ex)
            {
                await _logger.Error("ERD validation failed", ex);
                MessageBox.Show(
                    "ERD 검증 중 오류가 발생했습니다.",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<List<ValidationResult>> ValidateERDModel(ERDModel model)
        {
            var results = new List<ValidationResult>();

            if (model == null)
            {
                results.Add(new ValidationResult("Error", "모델", "ERD 모델이 비어있습니다."));
                return results;
            }

            // 테이블 검증
            foreach (var table in model.Tables)
            {
                // 테이블명 검사
                if (string.IsNullOrWhiteSpace(table.Name))
                {
                    results.Add(new ValidationResult("Error", "테이블", "테이블명이 비어있습니다."));
                    continue;
                }

                // 칼럼 존재 여부 검사
                if (!table.Columns.Any())
                {
                    results.Add(new ValidationResult(
                        "Error",
                        $"테이블: {table.Name}",
                        "테이블에 칼럼이 정의되지 않았습니다."));
                    continue;
                }

                // 기본키 존재 여부 검사
                if (!table.Columns.Any(c => c.IsPrimaryKey))
                {
                    results.Add(new ValidationResult(
                        "Warning",
                        $"테이블: {table.Name}",
                        "테이블에 기본키가 정의되지 않았습니다."));
                }

                // 칼럼 검증
                foreach (var column in table.Columns)
                {
                    // 칼럼명 검사
                    if (string.IsNullOrWhiteSpace(column.Name))
                    {
                        results.Add(new ValidationResult(
                            "Error",
                            $"테이블: {table.Name}",
                            "칼럼명이 비어있습니다."));
                    }

                    // 데이터 타입 검사
                    if (string.IsNullOrWhiteSpace(column.Type))
                    {
                        results.Add(new ValidationResult(
                            "Error",
                            $"테이블: {table.Name}, 칼럼: {column.Name}",
                            "데이터 타입이 정의되지 않았습니다."));
                    }

                    // 중복 칼럼명 검사
                    if (table.Columns.Count(c => c.Name == column.Name) > 1)
                    {
                        results.Add(new ValidationResult(
                            "Error",
                            $"테이블: {table.Name}",
                            $"중복된 칼럼명이 존재합니다: {column.Name}"));
                    }
                }
            }

            // 관계 검증
            foreach (var relationship in model.Relationships)
            {
                // 소스 테이블 존재 확인
                if (!model.Tables.Any(t => t.Name == relationship.SourceTable))
                {
                    results.Add(new ValidationResult(
                        "Error",
                        "관계",
                        $"소스 테이블이 존재하지 않습니다: {relationship.SourceTable}"));
                }

                // 타겟 테이블 존재 확인
                if (!model.Tables.Any(t => t.Name == relationship.TargetTable))
                {
                    results.Add(new ValidationResult(
                        "Error",
                        "관계",
                        $"타겟 테이블이 존재하지 않습니다: {relationship.TargetTable}"));
                }

                // 관계 타입 검사
                if (!IsValidRelationType(relationship.RelationType))
                {
                    results.Add(new ValidationResult(
                        "Error",
                        "관계",
                        $"잘못된 관계 타입입니다: {relationship.RelationType}"));
                }
            }

            await _logger.Info($"Model validation completed with {results.Count} findings");
            return results;
        }

        private bool IsValidRelationType(string relationType)
        {
            return relationType is "1-1" or "1-n" or "n-m";
        }

        private class ValidationResult
        {
            public string Severity { get; }
            public string Location { get; }
            public string Message { get; }

            public ValidationResult(string severity, string location, string message)
            {
                Severity = severity;
                Location = location;
                Message = message;
            }
        }
    }
}