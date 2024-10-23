using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using PC1MAINAITradingSystem.Core.ERD;
using PC1MAINAITradingSystem.Core.Database;
using PC1MAINAITradingSystem.Core.Security;
using PC1MAINAITradingSystem.Utils;
using PC1MAINAITradingSystem.Core.ERD.Base;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm : Form
    {
        private readonly ILogger _logger;
        private readonly IConfigManager _configManager;
        private readonly ERDCoreBase _erdCore;
        private readonly DatabaseManager _databaseManager;
        private readonly SecurityManager _securityManager;

        private readonly StatusStrip _statusStrip;
        private readonly MenuStrip _menuStrip;
        private readonly ToolStrip _toolStrip;
        private readonly Panel _mainPanel;
        private readonly Panel _sidePanel;
        private readonly SplitContainer _mainSplitContainer;
        private readonly TabControl _mainTabControl;

        private bool _isInitialized;

        public MainForm(
            ILogger logger,
            IConfigManager configManager,
            ERDCoreBase erdCore,
            DatabaseManager databaseManager,
            SecurityManager securityManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _erdCore = erdCore ?? throw new ArgumentNullException(nameof(erdCore));
            _databaseManager = databaseManager ?? throw new ArgumentNullException(nameof(databaseManager));
            _securityManager = securityManager ?? throw new ArgumentNullException(nameof(securityManager));

            InitializeComponent();

            // 기본 폼 설정
            Text = "ERD Manager";
            Size = new Size(1200, 800);
            MinimumSize = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;

            // 컴포넌트 초기화
            _statusStrip = CreateStatusStrip();
            _menuStrip = CreateMenuStrip();
            _toolStrip = CreateToolStrip();
            _mainPanel = CreateMainPanel();
            _sidePanel = CreateSidePanel();
            _mainSplitContainer = CreateMainSplitContainer();
            _mainTabControl = CreateMainTabControl();

            // 컨트롤 추가
            Controls.Add(_statusStrip);
            Controls.Add(_menuStrip);
            Controls.Add(_toolStrip);
            Controls.Add(_mainSplitContainer);

            // 이벤트 핸들러 등록
            Load += MainForm_Load;
            FormClosing += MainForm_FormClosing;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // 초기화 상태 표시
                UpdateStatus("Initializing...");

                // 시스템 초기화
                if (!await InitializeSystem())
                {
                    MessageBox.Show(
                        "Failed to initialize system. Please check the logs for details.",
                        "Initialization Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Close();
                    return;
                }

                // 사용자 인증
                if (!await AuthenticateUser())
                {
                    Close();
                    return;
                }

                _isInitialized = true;
                UpdateStatus("Ready");

                await _logger.LogInfo("Application initialized successfully");
            }
            catch (Exception ex)
            {
                await _logger.LogError("Failed to load main form", ex);
                MessageBox.Show(
                    "An error occurred while starting the application. Please check the logs for details.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
            }
        }

        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_isInitialized)
                {
                    // 변경사항 저장 확인
                    if (!await ConfirmSaveChanges())
                    {
                        e.Cancel = true;
                        return;
                    }

                    // 시스템 종료
                    await ShutdownSystem();
                }
            }
            catch (Exception ex)
            {
                await _logger.LogError("Error during application shutdown", ex);
                MessageBox.Show(
                    "An error occurred while closing the application. Please check the logs for details.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private StatusStrip CreateStatusStrip()
        {
            var statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom
            };

            statusStrip.Items.Add(new ToolStripStatusLabel
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            });

            statusStrip.Items.Add(new ToolStripStatusLabel
            {
                Alignment = ToolStripItemAlignment.Right,
                BorderSides = ToolStripStatusLabelBorderSides.Left,
                Width = 200
            });

            return statusStrip;
        }

        private MenuStrip CreateMenuStrip()
        {
            var menuStrip = new MenuStrip();

            // 파일 메뉴
            var fileMenu = CreateFileMenu();
            menuStrip.Items.Add(fileMenu);

            // 데이터베이스 메뉴
            var databaseMenu = CreateDatabaseMenu();
            menuStrip.Items.Add(databaseMenu);

            // ERD 메뉴
            var erdMenu = CreateERDMenu();
            menuStrip.Items.Add(erdMenu);

            // 도구 메뉴
            var toolsMenu = CreateToolsMenu();
            menuStrip.Items.Add(toolsMenu);

            // 도움말 메뉴
            var helpMenu = CreateHelpMenu();
            menuStrip.Items.Add(helpMenu);

            return menuStrip;
        }

        private ToolStrip CreateToolStrip()
        {
            var toolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden,
                RenderMode = ToolStripRenderMode.System
            };

            // 도구 모음 항목 추가
            toolStrip.Items.AddRange(CreateToolStripItems());

            return toolStrip;
        }

        private Panel CreateMainPanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill
            };
        }

        private Panel CreateSidePanel()
        {
            return new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private SplitContainer CreateMainSplitContainer()
        {
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 250,
                Panel1MinSize = 200,
                Panel2MinSize = 400
            };

            splitContainer.Panel1.Controls.Add(_sidePanel);
            splitContainer.Panel2.Controls.Add(_mainPanel);

            return splitContainer;
        }

        private TabControl CreateMainTabControl()
        {
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            _mainPanel.Controls.Add(tabControl);

            return tabControl;
        }

        private void UpdateStatus(string message)
        {
            if (_statusStrip.Items.Count > 0)
            {
                (_statusStrip.Items[0] as ToolStripStatusLabel).Text = message;
            }
        }

        private async Task<bool> InitializeSystem()
        {
            try
            {
                // 각 컴포넌트 초기화
                if (!await _erdCore.Initialize())
                    return false;

                if (!await _databaseManager.Initialize())
                    return false;

                if (!await _securityManager.Initialize())
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogError("System initialization failed", ex);
                return false;
            }
        }

        private async Task<bool> AuthenticateUser()
        {
            try
            {
                using var loginForm = new LoginForm(_securityManager);
                return loginForm.ShowDialog() == DialogResult.OK;
            }
            catch (Exception ex)
            {
                await _logger.LogError("User authentication failed", ex);
                return false;
            }
        }

        private async Task<bool> ConfirmSaveChanges()
        {
            // 변경사항 저장 확인 로직 구현
            return true;
        }

        private async Task ShutdownSystem()
        {
            try
            {
                // 시스템 종료 로직 구현
                await _logger.LogInfo("Application shutdown completed");
            }
            catch (Exception ex)
            {
                await _logger.LogError("System shutdown failed", ex);
                throw;
            }
        }

        // 계속해서 메뉴와 도구 모음 생성 메서드들을 구현할까요?
    }
}