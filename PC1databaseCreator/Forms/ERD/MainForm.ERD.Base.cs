using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using PC1MAINAITradingSystem.Core.Parser;
using PC1MAINAITradingSystem.Core.Generator;
using PC1MAINAITradingSystem.Core.Comparator;
using PC1MAINAITradingSystem.Core.ERD.Comparator;
using PC1MAINAITradingSystem.Core.ERD.Generator;
using PC1MAINAITradingSystem.Core.ERD.Parser;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private ERDParser _erdParser;
        private ERDGenerator _erdGenerator;
        private ERDComparer _erdComparer;
        private ERDModel _currentERD;
        private string _currentERDPath;
        private bool _isERDModified;

        private readonly TabControl _mainTabControl;
        private readonly Panel _erdVisualPanel;

        partial void InitializeERDBase()
        {
            _erdParser = new ERDParser();
            _erdGenerator = new ERDGenerator();
            _erdComparer = new ERDComparer();

            _mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            CreateTextView();
            CreateVisualView();

            Controls.Add(_mainTabControl);
        }

        private void CreateTextView()
        {
            var textTab = new TabPage("Text View");
            InitializeERDTextBox(textTab);
            _mainTabControl.TabPages.Add(textTab);
        }

        private void CreateVisualView()
        {
            var visualTab = new TabPage("Visual View");
            _erdVisualPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            visualTab.Controls.Add(_erdVisualPanel);
            _mainTabControl.TabPages.Add(visualTab);
        }

        private async Task HandleERDModification()
        {
            _isERDModified = true;
            await Task.WhenAll(
                UpdateVisualView(),
                _logger.Info("ERD content modified")
            );
        }

        private async Task<bool> CheckSaveChanges()
        {
            if (!_isERDModified) return true;

            var result = MessageBox.Show(
                "변경된 내용을 저장하시겠습니까?",
                "저장",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            switch (result)
            {
                case DialogResult.Yes:
                    return await SaveERD();
                case DialogResult.No:
                    return true;
                default:
                    return false;
            }
        }

        private async Task<bool> ValidateCurrentERD()
        {
            try
            {
                if (_currentERD == null)
                {
                    MessageBox.Show(
                        "ERD가 로드되지 않았습니다.",
                        "알림",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return false;
                }

                string content = GetERDContent();
                return await _erdParser.ValidateSyntax(content);
            }
            catch (Exception ex)
            {
                await _logger.Error("ERD validation failed", ex);
                return false;
            }
        }

        private string GetERDContent()
        {
            return _erdTextBox?.Text ?? string.Empty;
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            if (!await CheckSaveChanges())
            {
                e.Cancel = true;
                return;
            }

            await CleanupERD();
            base.OnFormClosing(e);
        }

        private async Task CleanupERD()
        {
            try
            {
                // 필요한 정리 작업 수행
                _currentERD = null;
                _currentERDPath = null;
                _isERDModified = false;

                await _logger.Info("ERD cleanup completed");
            }
            catch (Exception ex)
            {
                await _logger.Error("ERD cleanup failed", ex);
            }
        }
    }
}