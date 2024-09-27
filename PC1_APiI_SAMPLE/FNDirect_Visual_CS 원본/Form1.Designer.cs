namespace FNDirectTest
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DiConnectbutton = new System.Windows.Forms.Button();
            this.Connectbutton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Loginbutton = new System.Windows.Forms.Button();
            this.CerPWTextBox = new System.Windows.Forms.MaskedTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.PWTextBox = new System.Windows.Forms.MaskedTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.IDTextBox = new System.Windows.Forms.MaskedTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.Logoutbutton = new System.Windows.Forms.Button();
            this.IDOutTextBox = new System.Windows.Forms.MaskedTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.Procbutton = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.PorcTextBox = new System.Windows.Forms.RichTextBox();
            this.InputGrid = new System.Windows.Forms.PropertyGrid();
            this.ProclistView = new System.Windows.Forms.ListView();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.RealTextBox = new System.Windows.Forms.RichTextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.DiConnectbutton);
            this.groupBox1.Controls.Add(this.Connectbutton);
            this.groupBox1.Location = new System.Drawing.Point(1, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(335, 44);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "접속";
            // 
            // DiConnectbutton
            // 
            this.DiConnectbutton.Location = new System.Drawing.Point(137, 13);
            this.DiConnectbutton.Name = "DiConnectbutton";
            this.DiConnectbutton.Size = new System.Drawing.Size(87, 23);
            this.DiConnectbutton.TabIndex = 1;
            this.DiConnectbutton.Text = "Disconnect";
            this.DiConnectbutton.UseVisualStyleBackColor = true;
            this.DiConnectbutton.Click += new System.EventHandler(this.DiConnectbutton_Click);
            // 
            // Connectbutton
            // 
            this.Connectbutton.Location = new System.Drawing.Point(36, 13);
            this.Connectbutton.Name = "Connectbutton";
            this.Connectbutton.Size = new System.Drawing.Size(87, 23);
            this.Connectbutton.TabIndex = 0;
            this.Connectbutton.Text = "Connect";
            this.Connectbutton.UseVisualStyleBackColor = true;
            this.Connectbutton.Click += new System.EventHandler(this.Connectbutton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.Loginbutton);
            this.groupBox2.Controls.Add(this.CerPWTextBox);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.PWTextBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.IDTextBox);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(4, 47);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(507, 46);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Login";
            // 
            // Loginbutton
            // 
            this.Loginbutton.Location = new System.Drawing.Point(422, 16);
            this.Loginbutton.Name = "Loginbutton";
            this.Loginbutton.Size = new System.Drawing.Size(70, 23);
            this.Loginbutton.TabIndex = 3;
            this.Loginbutton.Text = "Login";
            this.Loginbutton.UseVisualStyleBackColor = true;
            this.Loginbutton.Click += new System.EventHandler(this.Loginbutton_Click);
            // 
            // CerPWTextBox
            // 
            this.CerPWTextBox.Location = new System.Drawing.Point(340, 17);
            this.CerPWTextBox.Name = "CerPWTextBox";
            this.CerPWTextBox.PasswordChar = '*';
            this.CerPWTextBox.Size = new System.Drawing.Size(72, 21);
            this.CerPWTextBox.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(252, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "Cert Password";
            // 
            // PWTextBox
            // 
            this.PWTextBox.Location = new System.Drawing.Point(171, 17);
            this.PWTextBox.Name = "PWTextBox";
            this.PWTextBox.PasswordChar = '*';
            this.PWTextBox.Size = new System.Drawing.Size(72, 21);
            this.PWTextBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(107, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Password";
            // 
            // IDTextBox
            // 
            this.IDTextBox.Location = new System.Drawing.Point(29, 17);
            this.IDTextBox.Name = "IDTextBox";
            this.IDTextBox.Size = new System.Drawing.Size(72, 21);
            this.IDTextBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(16, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "ID";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.Logoutbutton);
            this.groupBox3.Controls.Add(this.IDOutTextBox);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(514, 45);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(198, 48);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Logout";
            // 
            // Logoutbutton
            // 
            this.Logoutbutton.Location = new System.Drawing.Point(113, 18);
            this.Logoutbutton.Name = "Logoutbutton";
            this.Logoutbutton.Size = new System.Drawing.Size(70, 23);
            this.Logoutbutton.TabIndex = 0;
            this.Logoutbutton.Text = "Logout";
            this.Logoutbutton.UseVisualStyleBackColor = true;
            this.Logoutbutton.Click += new System.EventHandler(this.Logoutbutton_Click);
            // 
            // IDOutTextBox
            // 
            this.IDOutTextBox.Location = new System.Drawing.Point(35, 19);
            this.IDOutTextBox.Name = "IDOutTextBox";
            this.IDOutTextBox.ReadOnly = true;
            this.IDOutTextBox.Size = new System.Drawing.Size(72, 21);
            this.IDOutTextBox.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(16, 12);
            this.label4.TabIndex = 1;
            this.label4.Text = "ID";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.Procbutton);
            this.groupBox4.Controls.Add(this.groupBox5);
            this.groupBox4.Controls.Add(this.InputGrid);
            this.groupBox4.Controls.Add(this.ProclistView);
            this.groupBox4.Location = new System.Drawing.Point(4, 93);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(708, 392);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Proc";
            // 
            // Procbutton
            // 
            this.Procbutton.Location = new System.Drawing.Point(638, 20);
            this.Procbutton.Name = "Procbutton";
            this.Procbutton.Size = new System.Drawing.Size(64, 64);
            this.Procbutton.TabIndex = 2;
            this.Procbutton.Text = "요청";
            this.Procbutton.UseVisualStyleBackColor = true;
            this.Procbutton.Click += new System.EventHandler(this.Procbutton_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.PorcTextBox);
            this.groupBox5.Location = new System.Drawing.Point(1, 264);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(707, 127);
            this.groupBox5.TabIndex = 2;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Proc 응답";
            // 
            // PorcTextBox
            // 
            this.PorcTextBox.Location = new System.Drawing.Point(6, 17);
            this.PorcTextBox.Name = "PorcTextBox";
            this.PorcTextBox.Size = new System.Drawing.Size(686, 104);
            this.PorcTextBox.TabIndex = 3;
            this.PorcTextBox.Text = "";
            // 
            // InputGrid
            // 
            this.InputGrid.CommandsVisibleIfAvailable = false;
            this.InputGrid.HelpVisible = false;
            this.InputGrid.Location = new System.Drawing.Point(372, 20);
            this.InputGrid.Name = "InputGrid";
            this.InputGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.InputGrid.Size = new System.Drawing.Size(259, 238);
            this.InputGrid.TabIndex = 1;
            this.InputGrid.ToolbarVisible = false;
            // 
            // ProclistView
            // 
            this.ProclistView.AutoArrange = false;
            this.ProclistView.FullRowSelect = true;
            this.ProclistView.GridLines = true;
            this.ProclistView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ProclistView.HideSelection = false;
            this.ProclistView.Location = new System.Drawing.Point(8, 20);
            this.ProclistView.MultiSelect = false;
            this.ProclistView.Name = "ProclistView";
            this.ProclistView.Size = new System.Drawing.Size(357, 238);
            this.ProclistView.TabIndex = 0;
            this.ProclistView.UseCompatibleStateImageBehavior = false;
            this.ProclistView.View = System.Windows.Forms.View.Details;
            this.ProclistView.SelectedIndexChanged += new System.EventHandler(this.ProcListSelectChange);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.RealTextBox);
            this.groupBox6.Location = new System.Drawing.Point(8, 483);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(704, 127);
            this.groupBox6.TabIndex = 4;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Real 응답";
            // 
            // RealTextBox
            // 
            this.RealTextBox.Location = new System.Drawing.Point(1, 17);
            this.RealTextBox.Name = "RealTextBox";
            this.RealTextBox.Size = new System.Drawing.Size(686, 104);
            this.RealTextBox.TabIndex = 3;
            this.RealTextBox.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(718, 615);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1Closed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button DiConnectbutton;
        private System.Windows.Forms.Button Connectbutton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button Loginbutton;
        private System.Windows.Forms.MaskedTextBox CerPWTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MaskedTextBox PWTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MaskedTextBox IDTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button Logoutbutton;
        private System.Windows.Forms.MaskedTextBox IDOutTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RichTextBox PorcTextBox;
        private System.Windows.Forms.PropertyGrid InputGrid;
        private System.Windows.Forms.ListView ProclistView;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RichTextBox RealTextBox;


        private AxFNDirectLib.AxFNDirect m_fndirect;
        private bool m_bConnect;
        private bool m_bLogin;
        private int m_nListSel;
        private System.Windows.Forms.Button Procbutton;       
    }
}

