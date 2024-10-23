using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using PC1MAINAITradingSystem.Models.ERD;

namespace PC1MAINAITradingSystem.Forms.Controls
{
    public class ERDTableControl : UserControl
    {
        private readonly TableModel _tableModel;
        private readonly List<ERDColumnControl> _columnControls;
        private readonly Panel _headerPanel;
        private readonly Panel _columnsPanel;
        private readonly Label _titleLabel;
        private bool _isDragging;
        private Point _dragStartPoint;

        public event EventHandler<TableEventArgs> TableSelected;
        public event EventHandler<TableEventArgs> TableMoved;
        public event EventHandler<TableEventArgs> TableModified;
        public event EventHandler<TableEventArgs> TableDeleted;

        public TableModel TableModel => _tableModel;
        public bool IsSelected { get; private set; }

        private const int HEADER_HEIGHT = 30;
        private const int MIN_WIDTH = 200;
        private const int RESIZE_HANDLE_SIZE = 8;

        public ERDTableControl(TableModel table)
        {
            _tableModel = table ?? throw new ArgumentNullException(nameof(table));
            _columnControls = new List<ERDColumnControl>();

            // 컨트롤 초기화
            InitializeControl();

            // 패널 생성
            _headerPanel = CreateHeaderPanel();
            _columnsPanel = CreateColumnsPanel();
            _titleLabel = CreateTitleLabel();

            // 컬럼 컨트롤 생성
            CreateColumnControls();

            // 컨텍스트 메뉴 설정
            SetupContextMenu();

            // 이벤트 핸들러 등록
            SetupEventHandlers();
        }

        private void InitializeControl()
        {
            Size = new Size(MIN_WIDTH, HEADER_HEIGHT);
            MinimumSize = new Size(MIN_WIDTH, HEADER_HEIGHT);
            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;
            SetStyle(ControlStyles.ResizeRedraw |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer, true);
        }

        private Panel CreateHeaderPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = HEADER_HEIGHT,
                BackColor = Color.FromArgb(74, 158, 255)
            };

            Controls.Add(panel);
            return panel;
        }

        private Panel CreateColumnsPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            Controls.Add(panel);
            return panel;
        }

        private Label CreateTitleLabel()
        {
            var label = new Label
            {
                Text = _tableModel.Name,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
            };

            _headerPanel.Controls.Add(label);
            return label;
        }

        private void CreateColumnControls()
        {
            foreach (var column in _tableModel.Columns)
            {
                var columnControl = new ERDColumnControl(column)
                {
                    Dock = DockStyle.Top
                };

                columnControl.ColumnModified += OnColumnModified;
                columnControl.ColumnDeleted += OnColumnDeleted;

                _columnControls.Add(columnControl);
                _columnsPanel.Controls.Add(columnControl);
            }

            RecalculateSize();
        }

        private void SetupContextMenu()
        {
            var contextMenu = new ContextMenuStrip();

            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("테이블 편집(&E)", null, OnEditTable),
                new ToolStripMenuItem("컬럼 추가(&A)", null, OnAddColumn),
                new ToolStripSeparator(),
                new ToolStripMenuItem("테이블 삭제(&D)", null, OnDeleteTable),
                new ToolStripMenuItem("모든 관계 삭제(&R)", null, OnDeleteAllRelationships)
            });

            ContextMenuStrip = contextMenu;
        }

        private void SetupEventHandlers()
        {
            MouseDown += OnTableMouseDown;
            MouseMove += OnTableMouseMove;
            MouseUp += OnTableMouseUp;
            Paint += OnTablePaint;
            Resize += OnTableResize;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (IsSelected)
            {
                using var pen = new Pen(Color.Blue, 2);
                var rect = ClientRectangle;
                rect.Inflate(-1, -1);
                e.Graphics.DrawRectangle(pen, rect);
            }

            // 리사이즈 핸들 그리기
            if (IsSelected)
            {
                using var brush = new SolidBrush(Color.Blue);
                e.Graphics.FillRectangle(brush,
                    Width - RESIZE_HANDLE_SIZE,
                    Height - RESIZE_HANDLE_SIZE,
                    RESIZE_HANDLE_SIZE,
                    RESIZE_HANDLE_SIZE);
            }
        }

        private void OnTableMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _dragStartPoint = e.Location;
                BringToFront();

                // 테이블 선택
                Select();
                TableSelected?.Invoke(this, new TableEventArgs(_tableModel));
            }
        }

        private void OnTableMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var deltaX = e.X - _dragStartPoint.X;
                var deltaY = e.Y - _dragStartPoint.Y;

                Left += deltaX;
                Top += deltaY;

                TableMoved?.Invoke(this, new TableEventArgs(_tableModel));
            }
        }

        private void OnTableMouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        private void OnTablePaint(object sender, PaintEventArgs e)
        {
            // 선택 테두리 그리기
            if (IsSelected)
            {
                using var pen = new Pen(Color.Blue, 2);
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void OnTableResize(object sender, EventArgs e)
        {
            Width = Math.Max(Width, MIN_WIDTH);
            RecalculateSize();
        }

        private void OnEditTable(object sender, EventArgs e)
        {
            try
            {
                using var dialog = new TableEditorForm(_tableModel);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    UpdateTable(dialog.TableModel);
                    TableModified?.Invoke(this, new TableEventArgs(_tableModel));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"테이블 편집 중 오류가 발생했습니다: {ex.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnAddColumn(object sender, EventArgs e)
        {
            try
            {
                using var dialog = new ColumnEditorForm();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var column = dialog.ColumnModel;
                    _tableModel.Columns.Add(column);

                    var columnControl = new ERDColumnControl(column)
                    {
                        Dock = DockStyle.Top
                    };

                    columnControl.ColumnModified += OnColumnModified;
                    columnControl.ColumnDeleted += OnColumnDeleted;

                    _columnControls.Add(columnControl);
                    _columnsPanel.Controls.Add(columnControl);

                    RecalculateSize();
                    TableModified?.Invoke(this, new TableEventArgs(_tableModel));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"컬럼 추가 중 오류가 발생했습니다: {ex.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnDeleteTable(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                $"'{_tableModel.Name}' 테이블을 삭제하시겠습니까?",
                "테이블 삭제",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                TableDeleted?.Invoke(this, new TableEventArgs(_tableModel));
                Parent?.Controls.Remove(this);
            }
        }

        private void OnDeleteAllRelationships(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                $"'{_tableModel.Name}' 테이블의 모든 관계를 삭제하시겠습니까?",
                "관계 삭제",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // 관계 삭제 이벤트 발생
                // 실제 삭제는 ERD 모델에서 처리
                TableModified?.Invoke(this, new TableEventArgs(_tableModel));
            }
        }

        private void OnColumnModified(object sender, ColumnEventArgs e)
        {
            TableModified?.Invoke(this, new TableEventArgs(_tableModel));
        }

        private void OnColumnDeleted(object sender, ColumnEventArgs e)
        {
            if (sender is ERDColumnControl columnControl)
            {
                _tableModel.Columns.Remove(e.Column);
                _columnControls.Remove(columnControl);
                _columnsPanel.Controls.Remove(columnControl);

                RecalculateSize();
                TableModified?.Invoke(this, new TableEventArgs(_tableModel));
            }
        }

        private void UpdateTable(TableModel updatedTable)
        {
            _titleLabel.Text = updatedTable.Name;
            _tableModel.Name = updatedTable.Name;
            _tableModel.Description = updatedTable.Description;

            // 기타 속성 업데이트
            Invalidate();
        }

        private void RecalculateSize()
        {
            var totalHeight = HEADER_HEIGHT +
                            (_columnControls.Count * ERDColumnControl.COLUMN_HEIGHT);

            Height = Math.Max(totalHeight, MinimumSize.Height);
        }

        public void Select()
        {
            if (!IsSelected)
            {
                IsSelected = true;
                Invalidate();
            }
        }

        public void Deselect()
        {
            if (IsSelected)
            {
                IsSelected = false;
                Invalidate();
            }
        }
    }

    public class TableEventArgs : EventArgs
    {
        public TableModel Table { get; }

        public TableEventArgs(TableModel table)
        {
            Table = table;
        }
    }
}