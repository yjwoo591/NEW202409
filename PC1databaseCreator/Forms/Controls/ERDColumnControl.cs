using System;
using System.Drawing;
using System.Windows.Forms;
using PC1MAINAITradingSystem.Models.ERD;

namespace PC1MAINAITradingSystem.Forms.Controls
{
    public class ERDColumnControl : UserControl
    {
        private readonly ColumnModel _columnModel;
        private readonly Panel _iconPanel;
        private readonly Label _nameLabel;
        private readonly Label _typeLabel;
        private readonly PictureBox _primaryKeyIcon;
        private readonly PictureBox _foreignKeyIcon;
        private bool _isSelected;

        public event EventHandler<ColumnEventArgs> ColumnSelected;
        public event EventHandler<ColumnEventArgs> ColumnModified;
        public event EventHandler<ColumnEventArgs> ColumnDeleted;

        public const int COLUMN_HEIGHT = 25;
        private const int ICON_SIZE = 16;
        private const int PADDING = 4;

        public ColumnModel ColumnModel => _columnModel;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    UpdateAppearance();
                }
            }
        }

        public ERDColumnControl(ColumnModel column)
        {
            _columnModel = column ?? throw new ArgumentNullException(nameof(column));

            // 컨트롤 초기화
            InitializeControl();

            // 컴포넌트 생성
            _iconPanel = CreateIconPanel();
            _primaryKeyIcon = CreatePrimaryKeyIcon();
            _foreignKeyIcon = CreateForeignKeyIcon();
            _nameLabel = CreateNameLabel();
            _typeLabel = CreateTypeLabel();

            // 아이콘 패널에 아이콘 추가
            _iconPanel.Controls.AddRange(new Control[] { _primaryKeyIcon, _foreignKeyIcon });

            // 컨트롤에 컴포넌트 추가
            Controls.AddRange(new Control[] { _iconPanel, _nameLabel, _typeLabel });

            // 컨텍스트 메뉴 설정
            SetupContextMenu();

            // 이벤트 핸들러 등록
            SetupEventHandlers();

            // 초기 상태 업데이트
            UpdateAppearance();
        }

        private void InitializeControl()
        {
            Size = new Size(200, COLUMN_HEIGHT);
            MinimumSize = new Size(200, COLUMN_HEIGHT);
            MaximumSize = new Size(1000, COLUMN_HEIGHT);
            BackColor = Color.White;
            Padding = new Padding(PADDING);
        }

        private Panel CreateIconPanel()
        {
            return new Panel
            {
                Width = ICON_SIZE * 2 + PADDING,
                Height = COLUMN_HEIGHT,
                Dock = DockStyle.Left
            };
        }

        private PictureBox CreatePrimaryKeyIcon()
        {
            var icon = new PictureBox
            {
                Size = new Size(ICON_SIZE, ICON_SIZE),
                Location = new Point(0, (COLUMN_HEIGHT - ICON_SIZE) / 2),
                Image = Properties.Resources.PrimaryKeyIcon,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = _columnModel.IsPrimaryKey
            };

            icon.MouseEnter += (s, e) => ShowTooltip(icon, "기본 키");
            return icon;
        }

        private PictureBox CreateForeignKeyIcon()
        {
            var icon = new PictureBox
            {
                Size = new Size(ICON_SIZE, ICON_SIZE),
                Location = new Point(ICON_SIZE + PADDING, (COLUMN_HEIGHT - ICON_SIZE) / 2),
                Image = Properties.Resources.ForeignKeyIcon,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = _columnModel.IsForeignKey
            };

            icon.MouseEnter += (s, e) => ShowTooltip(icon, "외래 키");
            return icon;
        }

        private Label CreateNameLabel()
        {
            return new Label
            {
                Text = _columnModel.Name,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Width = 100,
                Height = COLUMN_HEIGHT - (PADDING * 2),
                Location = new Point(_iconPanel.Width + PADDING, PADDING),
                Font = new Font(Font.FontFamily, 9)
            };
        }

        private Label CreateTypeLabel()
        {
            return new Label
            {
                Text = FormatDataType(_columnModel),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Width = 80,
                Height = COLUMN_HEIGHT - (PADDING * 2),
                Location = new Point(Width - 85, PADDING),
                Font = new Font(Font.FontFamily, 8),
                ForeColor = Color.Gray
            };
        }

        private void SetupContextMenu()
        {
            var contextMenu = new ContextMenuStrip();

            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("컬럼 편집(&E)", null, OnEditColumn),
                new ToolStripMenuItem("기본 키로 설정(&P)", null, OnSetPrimaryKey)
                {
                    Checked = _columnModel.IsPrimaryKey
                },
                new ToolStripMenuItem("외래 키로 설정(&F)", null, OnSetForeignKey)
                {
                    Checked = _columnModel.IsForeignKey
                },
                new ToolStripSeparator(),
                new ToolStripMenuItem("컬럼 삭제(&D)", null, OnDeleteColumn)
            });

            ContextMenuStrip = contextMenu;
        }

        private void SetupEventHandlers()
        {
            MouseClick += OnColumnMouseClick;
            MouseEnter += OnColumnMouseEnter;
            MouseLeave += OnColumnMouseLeave;
            Paint += OnColumnPaint;
        }

        private void UpdateAppearance()
        {
            _primaryKeyIcon.Visible = _columnModel.IsPrimaryKey;
            _foreignKeyIcon.Visible = _columnModel.IsForeignKey;
            _nameLabel.Text = _columnModel.Name;
            _typeLabel.Text = FormatDataType(_columnModel);

            BackColor = _isSelected ? Color.FromArgb(230, 240, 255) : Color.White;
            Invalidate();
        }

        private string FormatDataType(ColumnModel column)
        {
            var typeStr = column.DataType.ToString().ToLower();

            if (column.Length.HasValue)
            {
                typeStr += $"({column.Length})";
            }
            else if (column.Precision.HasValue && column.Scale.HasValue)
            {
                typeStr += $"({column.Precision},{column.Scale})";
            }

            return typeStr;
        }

        private void ShowTooltip(Control control, string text)
        {
            var toolTip = new ToolTip();
            toolTip.SetToolTip(control, text);
        }

        private void OnColumnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Select();
                ColumnSelected?.Invoke(this, new ColumnEventArgs(_columnModel));
            }
        }

        private void OnColumnMouseEnter(object sender, EventArgs e)
        {
            if (!_isSelected)
            {
                BackColor = Color.FromArgb(245, 245, 245);
            }
        }

        private void OnColumnMouseLeave(object sender, EventArgs e)
        {
            if (!_isSelected)
            {
                BackColor = Color.White;
            }
        }

        private void OnColumnPaint(object sender, PaintEventArgs e)
        {
            if (_isSelected)
            {
                using var pen = new Pen(Color.FromArgb(0, 120, 215), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void OnEditColumn(object sender, EventArgs e)
        {
            try
            {
                using var dialog = new ColumnEditorForm(_columnModel);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    UpdateColumn(dialog.ColumnModel);
                    ColumnModified?.Invoke(this, new ColumnEventArgs(_columnModel));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"컬럼 편집 중 오류가 발생했습니다: {ex.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnSetPrimaryKey(object sender, EventArgs e)
        {
            _columnModel.IsPrimaryKey = !_columnModel.IsPrimaryKey;
            if (_columnModel.IsPrimaryKey)
            {
                _columnModel.IsNullable = false;
            }
            UpdateAppearance();
            ColumnModified?.Invoke(this, new ColumnEventArgs(_columnModel));
        }

        private void OnSetForeignKey(object sender, EventArgs e)
        {
            try
            {
                using var dialog = new ForeignKeyEditorForm(_columnModel);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    UpdateColumn(dialog.ColumnModel);
                    ColumnModified?.Invoke(this, new ColumnEventArgs(_columnModel));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"외래 키 설정 중 오류가 발생했습니다: {ex.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnDeleteColumn(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                $"'{_columnModel.Name}' 컬럼을 삭제하시겠습니까?",
                "컬럼 삭제",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ColumnDeleted?.Invoke(this, new ColumnEventArgs(_columnModel));
            }
        }

        private void UpdateColumn(ColumnModel updatedColumn)
        {
            _columnModel.Name = updatedColumn.Name;
            _columnModel.DataType = updatedColumn.DataType;
            _columnModel.Length = updatedColumn.Length;
            _columnModel.Precision = updatedColumn.Precision;
            _columnModel.Scale = updatedColumn.Scale;
            _columnModel.IsNullable = updatedColumn.IsNullable;
            _columnModel.DefaultValue = updatedColumn.DefaultValue;
            _columnModel.Description = updatedColumn.Description;

            UpdateAppearance();
        }
    }

    public class ColumnEventArgs : EventArgs
    {
        public ColumnModel Column { get; }

        public ColumnEventArgs(ColumnModel column)
        {
            Column = column;
        }
    }
}