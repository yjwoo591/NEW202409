using System;
using System.Windows.Forms;

namespace PC1MAINAITradingSystem.Forms
{
    public class ERDViewerForm : Form
    {
        private PictureBox erdPictureBox;

        public ERDViewerForm(string erdContent)
        {
            InitializeComponent();
            DisplayERD(erdContent);
        }

        private void InitializeComponent()
        {
            this.Text = "ERD Viewer";
            this.Size = new System.Drawing.Size(800, 600);

            erdPictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            this.Controls.Add(erdPictureBox);
        }

        private void DisplayERD(string erdContent)
        {
            // 여기서 erdContent를 이미지로 변환하여 erdPictureBox에 표시
            // 실제 구현은 ERD 생성 라이브러리에 따라 달라질 수 있음
        }
    }
}