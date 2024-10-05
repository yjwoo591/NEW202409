using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Imaging;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace ForexAITradingPC1Main.Forms
{
    public class ERDMermaidVisualizer : Form
    {
        private TextBox mermaidTextBox;
        private WebBrowser previewBrowser;
        private Button updateButton;
        private Button saveButton;
        private Button loadButton;
        private Button exportButton;
        private ComboBox styleComboBox;
        private const string DEFAULT_ERD_FILENAME = "PC1ERD.mmd";
        private const string PROJECT_FOLDER = @"projectDEV\Database";
        private const string BACKUP_FOLDER = @"projectDEV\Database\ERD Backup";

        public ERDMermaidVisualizer()
        {
            InitializeComponent();
            LoadDefaultERD();
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(1200, 800);
            this.Text = "ERD Mermaid Visualizer";

            mermaidTextBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Left,
                Width = 400
            };
            this.Controls.Add(mermaidTextBox);

            previewBrowser = new WebBrowser
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(previewBrowser);

            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100
            };
            this.Controls.Add(buttonPanel);

            updateButton = new Button
            {
                Text = "Update Preview",
                Location = new Point(10, 10),
                Size = new Size(120, 30)
            };
            updateButton.Click += UpdateButton_Click;
            buttonPanel.Controls.Add(updateButton);

            saveButton = new Button
            {
                Text = "Save Mermaid",
                Location = new Point(140, 10),
                Size = new Size(120, 30)
            };
            saveButton.Click += SaveButton_Click;
            buttonPanel.Controls.Add(saveButton);

            loadButton = new Button
            {
                Text = "Load Mermaid",
                Location = new Point(270, 10),
                Size = new Size(120, 30)
            };
            loadButton.Click += LoadButton_Click;
            buttonPanel.Controls.Add(loadButton);

            exportButton = new Button
            {
                Text = "Export",
                Location = new Point(400, 10),
                Size = new Size(120, 30)
            };
            exportButton.Click += ExportButton_Click;
            buttonPanel.Controls.Add(exportButton);

            styleComboBox = new ComboBox
            {
                Location = new Point(10, 50),
                Size = new Size(200, 30)
            };
            styleComboBox.Items.AddRange(new object[] { "Default", "Forest", "Neutral", "Dark" });
            styleComboBox.SelectedIndex = 0;
            styleComboBox.SelectedIndexChanged += StyleComboBox_SelectedIndexChanged;
            buttonPanel.Controls.Add(styleComboBox);
        }

        private void LoadDefaultERD()
        {
            string defaultErdPath = Path.Combine(PROJECT_FOLDER, DEFAULT_ERD_FILENAME);
            if (File.Exists(defaultErdPath))
            {
                mermaidTextBox.Text = File.ReadAllText(defaultErdPath);
            }
            else
            {
                GenerateDefaultERD();
            }
            UpdatePreview();
        }

        private void GenerateDefaultERD()
        {
            // Generate a default ERD here
            mermaidTextBox.Text = @"erDiagram
    CUSTOMER {
        int Id PK
        string Name
        string Email
    }
    ACCOUNT {
        int Id PK
        string AccountNumber
        int CustomerId FK
    }
    CUSTOMER ||--o{ ACCOUNT : has";
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            string selectedTheme = styleComboBox.SelectedItem.ToString().ToLower();
            string html = $@"
                <html>
                <body>
                    <script src=""https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.min.js""></script>
                    <script>
                        mermaid.initialize({{ 
                            startOnLoad: true,
                            theme: '{selectedTheme}'
                        }});
                    </script>
                    <div class=""mermaid"">
                        {mermaidTextBox.Text}
                    </div>
                </body>
                </html>";

            string tempPath = Path.GetTempFileName() + ".html";
            File.WriteAllText(tempPath, html);
            previewBrowser.Navigate(tempPath);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string erdPath = Path.Combine(PROJECT_FOLDER, DEFAULT_ERD_FILENAME);

            // Create backup
            if (File.Exists(erdPath))
            {
                string backupFileName = $"PC1ERD_BK_{DateTime.Now:yyMMddHHmmss}.mmd";
                string backupPath = Path.Combine(BACKUP_FOLDER, backupFileName);
                Directory.CreateDirectory(BACKUP_FOLDER);
                File.Move(erdPath, backupPath);
            }

            // Save new file
            File.WriteAllText(erdPath, mermaidTextBox.Text);
            MessageBox.Show("ERD saved successfully!", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            string erdPath = Path.Combine(PROJECT_FOLDER, DEFAULT_ERD_FILENAME);
            if (File.Exists(erdPath))
            {
                mermaidTextBox.Text = File.ReadAllText(erdPath);
                UpdatePreview();
            }
            else
            {
                MessageBox.Show("Default ERD file not found.", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|PDF Document|*.pdf",
                Title = "Export ERD"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog.FileName.EndsWith(".png"))
                {
                    ExportToPng(saveFileDialog.FileName);
                }
                else if (saveFileDialog.FileName.EndsWith(".pdf"))
                {
                    ExportToPdf(saveFileDialog.FileName);
                }
            }
        }

        private void ExportToPng(string filePath)
        {
            previewBrowser.DrawToBitmap(new Bitmap(previewBrowser.Width, previewBrowser.Height), new Rectangle(0, 0, previewBrowser.Width, previewBrowser.Height));
            Bitmap bmp = new Bitmap(previewBrowser.Width, previewBrowser.Height);
            previewBrowser.DrawToBitmap(bmp, new Rectangle(0, 0, previewBrowser.Width, previewBrowser.Height));
            bmp.Save(filePath, ImageFormat.Png);
            MessageBox.Show("ERD exported as PNG successfully!", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportToPdf(string filePath)
        {
            PdfDocument pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(previewBrowser.DocumentText, PdfSharp.PageSize.A4);
            pdf.Save(filePath);
            MessageBox.Show("ERD exported as PDF successfully!", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void StyleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }
    }
}
/*
 * 이 EDMermaidVisualizer 클래스는 다음과 같은 주요 기능을 포함하고 있습니다:

Mermaid 형식의 ERD를 편집할 수 있는 텍스트 상자
ERD의 실시간 미리보기
ERD 저장 및 로드 기능
PNG 및 PDF 형식으로 ERD가 있습니다.
다양한 스타일 (Default, Forest, Neutral, Dark) 선택 옵션

이 클래스는 Windows Forms의 일부로, ERD를 표시하여 편집하고 관리하는 데 사용됩니다. 
만약 이 파일의 내용이 귀하의 현재 포함 및 변경면이라면, 어떤 부분이 지정되어야 하는지 더 자세히 설명하기 위해 필요한 수정 사항을 제안해 드릴 것입니다.
 * 
 * 
 * 이 ERDMermaidVisualizer.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

Mermaid Format의 ERD를 편집, 미리보기, 저장 및 로드하는 기능을 제공합니다.
ERD를 PNG 또는 PDF 형식으로 처리하는 기능이 포함됩니다.
다양한 스타일(Default, Forest, Neutral, Dark)을 제외할 수 있는 옵션을 제공합니다.
기본적으로 ERD 버퍼를 생성하고 관리하는 기능이 있습니다.
ERD 파일의 백업을 자동으로 생성합니다.

주요 방법:

InitializeComponent(): UI 구성요소를 저장합니다.
LoadDefaultERD(): 기본 ERD 파일을 로드하거나 생성합니다.
UpdatePreview(): ERD 미리보기를 업데이트합니다.
SaveButton_Click(): 현재 ERD를 저장하고 백업을 생성합니다.
LoadButton_Click(): 저장된 ERD를 로드합니다.
ImportToPng(): ERD를 PNG 이미지로 내보냅니다.
ImportToPdf(): ERD를 PDF 문서로 내보냅니다.

이 클래스를 사용하여 ERD를 표시하고 관리하고, 다양한 형식으로 내보낼 수 있습니다. PdfSharp 라이브러리를 사용하고 있는지 여부를 확인해야 합니다. 프로젝트에 PdfSharp와 HtmlRenderer.PdfSharp NuGet 패키지가 포함되어 있는지 확인해야 합니다.
추가 기능이나 수정이 필요하시면 말씀해 주세요.




 * 이 ERDMermaidVisualizer.cs 파일은 ForexAITradingPC1Main.Forms 접착 공간 내부에 정의되어 있으며 다음과 같은 주요 특징을 가지고 있습니다.

Mermaid Format의 ERD를 텍스트로 표시하고 편집할 수 있는 텍스트 박스를 제공합니다.
ERD의 업데이트 내용을 웹 브라우저에 포함시키세요.
미리 보기 업데이트, Mermaid 파일 저장 및 로드 기능을 제공합니다.

주요 기능:

generateMermaidERD(): 초기 Mermaid ERD를 생성합니다.
UpdatePreview(): 현재 Mermaid 텍스트를 기반으로 미리 보기를 업데이트합니다.
SaveButton_Click(): 현재 Mermaid텍스트를 파일로 저장합니다.
LoadButton_Click(): 저장된 Mermaid 파일을 로드합니다.

이 폼은 사용자가 ERD를 표시로 확인하고 편집할 수 있게 되었습니다. 또한 Mermaid 파일을 저장하고 로드하는 기능을 통해 ERD를 쉽게 관리할 수 있습니다.
추가적인 개선 사항이나 기능이 필요하시면 말씀해 주십시오. 예를 들어, 다음과 같은 기능을 추가할 수 있습니다:

ERD 편집을 사용자 인터페이스 인터페이스
데이터베이스와 Mermaid ERD
다양한 ERD 스타일 옵션
ERD를 이미지나 PDF로 포함할 수 있습니다.

필요한 경우 이러한 기능을 구현하거나 기존 기능을 사용할 수 있습니다.
*/