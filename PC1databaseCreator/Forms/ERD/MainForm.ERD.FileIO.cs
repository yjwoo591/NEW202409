using System;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private async Task<bool> LoadERD()
        {
            try
            {
                if (!await CheckSaveChanges()) return false;

                using var openFileDialog = new OpenFileDialog
                {
                    Filter = "ERD 파일 (*.mermaid)|*.mermaid|모든 파일 (*.*)|*.*",
                    FilterIndex = 1
                };

                if (openFileDialog.ShowDialog() != DialogResult.OK) return false;

                await LoadERDFile(openFileDialog.FileName);
                return true;
            }
            catch (Exception ex)
            {
                await _logger.Error("Failed to load ERD", ex);
                MessageBox.Show(
                    "ERD 파일을 불러오는 중 오류가 발생했습니다.",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task LoadERDFile(string filePath)
        {
            try
            {
                string content = await File.ReadAllTextAsync(filePath);

                // ERD 파싱 및 검증
                if (await _erdParser.ValidateSyntax(content))
                {
                    _currentERD = await _erdParser.ParseMermaid(content);
                    _currentERDPath = filePath;
                    _erdTextBox.Text = content;
                    _isERDModified = false;

                    await Task.WhenAll(
                        UpdateVisualView(),
                        _logger.Info($"ERD file loaded: {filePath}")
                    );

                    UpdateFormTitle();
                }
            }
            catch (Exception ex)
            {
                await _logger.Error($"Failed to load ERD file: {filePath}", ex);
                throw;
            }
        }

        private async Task<bool> SaveERD()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentERDPath))
                {
                    return await SaveERDAs();
                }

                await SaveERDFile(_currentERDPath);
                return true;
            }
            catch (Exception ex)
            {
                await _logger.Error("Failed to save ERD", ex);
                MessageBox.Show(
                    "ERD 파일 저장 중 오류가 발생했습니다.",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> SaveERDAs()
        {
            try
            {
                using var saveFileDialog = new SaveFileDialog
                {
                    Filter = "ERD 파일 (*.mermaid)|*.mermaid|모든 파일 (*.*)|*.*",
                    FilterIndex = 1,
                    DefaultExt = "mermaid"
                };

                if (saveFileDialog.ShowDialog() != DialogResult.OK) return false;

                await SaveERDFile(saveFileDialog.FileName);
                return true;
            }
            catch (Exception ex)
            {
                await _logger.Error("Failed to save ERD as new file", ex);
                MessageBox.Show(
                    "ERD 파일 저장 중 오류가 발생했습니다.",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task SaveERDFile(string filePath)
        {
            try
            {
                // 백업 파일 생성
                if (File.Exists(filePath))
                {
                    string backupPath = $"{filePath}.{DateTime.Now:yyyyMMddHHmmss}.bak";
                    File.Copy(filePath, backupPath);
                }

                // 현재 내용 저장
                await File.WriteAllTextAsync(filePath, _erdTextBox.Text);
                _currentERDPath = filePath;
                _isERDModified = false;

                await _logger.Info($"ERD file saved: {filePath}");
                UpdateFormTitle();
            }
            catch (Exception ex)
            {
                await _logger.Error($"Failed to save ERD file: {filePath}", ex);
                throw;
            }
        }

        private async Task<bool> ExportERD(string format)
        {
            try
            {
                if (!await ValidateERDContent()) return false;

                using var saveFileDialog = new SaveFileDialog
                {
                    Filter = GetExportFilter(format),
                    FilterIndex = 1,
                    DefaultExt = format.ToLower()
                };

                if (saveFileDialog.ShowDialog() != DialogResult.OK) return false;

                await ExportERDFile(saveFileDialog.FileName, format);
                return true;
            }
            catch (Exception ex)
            {
                await _logger.Error($"Failed to export ERD as {format}", ex);
                MessageBox.Show(
                    $"ERD를 {format} 형식으로 내보내는 중 오류가 발생했습니다.",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private string GetExportFilter(string format)
        {
            return format.ToUpper() switch
            {
                "PNG" => "PNG 이미지 (*.png)|*.png",
                "SVG" => "SVG 벡터 이미지 (*.svg)|*.svg",
                "PDF" => "PDF 문서 (*.pdf)|*.pdf",
                _ => "모든 파일 (*.*)|*.*"
            };
        }

        private async Task ExportERDFile(string filePath, string format)
        {
            try
            {
                switch (format.ToUpper())
                {
                    case "PNG":
                        await ExportAsPng(filePath);
                        break;
                    case "SVG":
                        await ExportAsSvg(filePath);
                        break;
                    case "PDF":
                        await ExportAsPdf(filePath);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported export format: {format}");
                }

                await _logger.Info($"ERD exported as {format}: {filePath}");
            }
            catch (Exception ex)
            {
                await _logger.Error($"Failed to export ERD file: {filePath}", ex);
                throw;
            }
        }

        private async Task ExportAsPng(string filePath)
        {
            // PNG 내보내기 구현
            throw new NotImplementedException();
        }

        private async Task ExportAsSvg(string filePath)
        {
            // SVG 내보내기 구현
            throw new NotImplementedException();
        }

        private async Task ExportAsPdf(string filePath)
        {
            // PDF 내보내기 구현
            throw new NotImplementedException();
        }

        private void UpdateFormTitle()
        {
            string fileName = Path.GetFileName(_currentERDPath) ?? "Untitled";
            Text = $"ERD Manager - {fileName}{(_isERDModified ? "*" : "")}";
        }
    }
}