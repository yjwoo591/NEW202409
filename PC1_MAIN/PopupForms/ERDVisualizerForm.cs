using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows.Forms;
using ForexAITradingSystem.Properties;

namespace ForexAITradingSystem.PopupForms
{
    public partial class ERDVisualizerForm : Form
    {
        private WebView2 webView;
        private TextBox txtERDCode;
        private Button btnUpdateERD;
        private Button btnSave;
        private Button btnLoad;
        private string erdCode;

        public ERDVisualizerForm()
        {
            InitializeComponent();
            LoadWindowSettings();
            InitializeERDCode();
            InitializeWebView();
        }

        private void InitializeComponent()
        {
            this.webView = new WebView2();
            this.txtERDCode = new TextBox();
            this.btnUpdateERD = new Button();
            this.btnSave = new Button();
            this.btnLoad = new Button();

            this.txtERDCode.Multiline = true;
            this.txtERDCode.ScrollBars = ScrollBars.Vertical;
            this.txtERDCode.Dock = DockStyle.Bottom;
            this.txtERDCode.Height = 200;

            this.btnUpdateERD.Text = "Update ERD";
            this.btnUpdateERD.Dock = DockStyle.Bottom;
            this.btnUpdateERD.Click += BtnUpdateERD_Click;

            this.btnSave.Text = "Save";
            this.btnSave.Dock = DockStyle.Bottom;
            this.btnSave.Click += BtnSave_Click;

            this.btnLoad.Text = "Load";
            this.btnLoad.Dock = DockStyle.Bottom;
            this.btnLoad.Click += BtnLoad_Click;

            this.webView.Dock = DockStyle.Fill;

            this.Controls.Add(this.webView);
            this.Controls.Add(this.txtERDCode);
            this.Controls.Add(this.btnUpdateERD);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnLoad);

            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "ERD Visualizer";
        }

        private void InitializeERDCode()
        {
            erdCode = @"
erDiagram
    PRODUCT_TYPE {
        int code PK
        string name
        string description
        int month_type
    }
    YEAR_CODE {
        char code PK
        int year
    }
    MONTH_CODE {
        char code PK
        int month
    }
    PRODUCT {
        int id PK
        int product_type_code FK
        char year_code FK
        char month_code FK
        date expiry_date
        string ticker
        decimal current_price
    }
    TRADING_SESSION {
        int id PK
        date date
        boolean is_trading_day
    }
    ORDER {
        int id PK
        int product_id FK
        int account_id FK
        decimal amount
        string order_type
        datetime order_date
    }

    PRODUCT_TYPE ||--o{ PRODUCT : ""defines""
    YEAR_CODE ||--o{ PRODUCT : ""belongs to""
    MONTH_CODE ||--o{ PRODUCT : ""belongs to""
    PRODUCT ||--o{ ORDER : ""has""
    TRADING_SESSION ||--o{ PRODUCT : ""affects expiry""

         ";
            txtERDCode.Text = erdCode;
        }

        private async void InitializeWebView()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.Settings.IsScriptEnabled = true;
            webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
            webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
            webView.WebMessageReceived += WebView_WebMessageReceived;

            webView.NavigationCompleted += WebView_NavigationCompleted;
            UpdateERD();
        }

        private void LoadWindowSettings()
        {
            if (Settings.Default.ERDVisualizerLocation != Point.Empty)
            {
                this.Location = Settings.Default.ERDVisualizerLocation;
            }
            if (Settings.Default.ERDVisualizerSize != Size.Empty)
            {
                this.Size = Settings.Default.ERDVisualizerSize;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            SaveWindowSettings();
        }

        private void SaveWindowSettings()
        {
            Settings.Default.ERDVisualizerLocation = this.Location;
            Settings.Default.ERDVisualizerSize = this.Size;
            Settings.Default.Save();
        }
        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                Console.WriteLine("Navigation completed successfully.");
            }
            else
            {
                Console.WriteLine($"Navigation failed with error: {e.WebErrorStatus}");
            }
        }

        private void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string message = e.TryGetWebMessageAsString();
            dynamic data = JsonConvert.DeserializeObject(message);
            string action = data.action;
            string id = data.id;

            switch (action)
            {
                case "editTable":
                    EditTable(id);
                    break;
                case "editRelation":
                    EditRelation(id);
                    break;
            }
        }

        private void EditTable(string tableId)
        {
            // 테이블 편집 대화상자를 표시하는 코드
            MessageBox.Show($"Edit table: {tableId}");
        }

        private void EditRelation(string relationId)
        {
            // 관계 편집 대화상자를 표시하는 코드
            MessageBox.Show($"Edit relation: {relationId}");
        }

        private void BtnUpdateERD_Click(object sender, EventArgs e)
        {
            UpdateERD();
        }

        private async void UpdateERD()
        {
            erdCode = txtERDCode.Text;
            string html = $@"
<!DOCTYPE html>
<html>
<head>
    <script src=""https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.min.js""></script>
    <style>
        .mermaid {{
            cursor: pointer;
        }}
    </style>
    <script>
        mermaid.initialize({{ startOnLoad: true }});
        
        function addClickListeners() {{
            document.querySelectorAll('.mermaid .node').forEach(node => {{
                node.onclick = function() {{
                    window.chrome.webview.postMessage(JSON.stringify({{
                        action: 'editTable',
                        id: this.id
                    }}));
                }};
            }});
            
            document.querySelectorAll('.mermaid .edgePath').forEach(edge => {{
                edge.onclick = function() {{
                    window.chrome.webview.postMessage(JSON.stringify({{
                        action: 'editRelation',
                        id: this.id
                    }}));
                }};
            }});
        }}

        document.addEventListener('DOMContentLoaded', (event) => {{
            setTimeout(addClickListeners, 1000); // mermaid 렌더링 후 리스너 추가
        }});
    </script>
</head>
<body>
    <div class=""mermaid"">
    {erdCode}
    </div>
</body>
</html>";

            await webView.EnsureCoreWebView2Async(null);
            webView.NavigateToString(html);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveERDCode();
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            LoadERDCode();
        }

        private void SaveERDCode()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JSON files (*.json)|*.json";
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string json = JsonConvert.SerializeObject(new { ERDCode = erdCode });
                    File.WriteAllText(saveFileDialog.FileName, json);
                }
            }
        }

        private void LoadERDCode()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON files (*.json)|*.json";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string json = File.ReadAllText(openFileDialog.FileName);
                    var data = JsonConvert.DeserializeObject<dynamic>(json);
                    erdCode = data.ERDCode;
                    txtERDCode.Text = erdCode;
                    UpdateERD();
                }
            }
        }
    }
}