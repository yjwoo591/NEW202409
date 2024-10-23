using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PC1MAINAITradingSystem.Models.ERD;

namespace PC1MAINAITradingSystem.Forms.Controls
{
    public class ERDRelationControl : Control
    {
        private readonly RelationshipModel _relationshipModel;
        private readonly ERDTableControl _sourceTable;
        private readonly ERDTableControl _targetTable;
        private bool _isSelected;
        private bool _isHovered;
        private Rectangle _hitArea;

        public event EventHandler<RelationEventArgs> RelationSelected;
        public event EventHandler<RelationEventArgs> RelationModified;
        public event EventHandler<RelationEventArgs> RelationDeleted;

        public RelationshipModel RelationshipModel => _relationshipModel;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    Invalidate();
                }
            }
        }

        private const int LINE_THICKNESS = 2;
        private const int ARROW_SIZE = 10;
        private const int HIT_AREA_WIDTH = 10;

        public ERDRelationControl(
            RelationshipModel relationship,
            ERDTableControl sourceTable,
            ERDTableControl targetTable)
        {
            _relationshipModel = relationship ?? throw new ArgumentNullException(nameof(relationship));
            _sourceTable = sourceTable ?? throw new ArgumentNullException(nameof(sourceTable));
            _targetTable = targetTable ?? throw new ArgumentNullException(nameof(targetTable));

            InitializeControl();
            SetupContextMenu();
            SetupEventHandlers();
        }

        private void InitializeControl()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
        }

        private void SetupContextMenu()
        {
            var contextMenu = new ContextMenuStrip();

            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("관계 편집(&E)", null, OnEditRelation),
                new ToolStripSeparator(),
                new ToolStripMenuItem("관계 유형", null, null, new ToolStripItem[]
                {
                    new ToolStripMenuItem("1:1", null, (s, e) => ChangeRelationType("1-1")),
                    new ToolStripMenuItem("1:N", null, (s, e) => ChangeRelationType("1-n")),
                    new ToolStripMenuItem("N:M", null, (s, e) => ChangeRelationType("n-m"))
                }),
                new ToolStripSeparator(),
                new ToolStripMenuItem("관계 삭제(&D)", null, OnDeleteRelation)
            });

            ContextMenuStrip = contextMenu;
        }

        private void SetupEventHandlers()
        {
            MouseClick += OnRelationMouseClick;
            MouseEnter += OnRelationMouseEnter;
            MouseLeave += OnRelationMouseLeave;
            Paint += OnRelationPaint;

            _sourceTable.LocationChanged += OnTableLocationChanged;
            _targetTable.LocationChanged += OnTableLocationChanged;
            _sourceTable.SizeChanged += OnTableSizeChanged;
            _targetTable.SizeChanged += OnTableSizeChanged;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var (startPoint, endPoint) = CalculateConnectionPoints();
            var lineColor = GetLineColor();
            var lineStyle = GetLineStyle();

            using var pen = new Pen(lineColor, LINE_THICKNESS)
            {
                DashStyle = lineStyle
            };

            // 관계선 그리기
            DrawRelationLine(e.Graphics, startPoint, endPoint, pen);

            // 관계 타입에 따른 끝점 표시 그리기
            DrawRelationEndpoints(e.Graphics, startPoint, endPoint, lineColor);

            // 관계 레이블 그리기
            DrawRelationLabel(e.Graphics, startPoint, endPoint);

            // 히트 영역 업데이트
            UpdateHitArea(startPoint, endPoint);
        }

        private void DrawRelationLine(Graphics g, Point start, Point end, Pen pen)
        {
            var controlPoints = CalculateControlPoints(start, end);
            g.DrawBezier(pen, start, controlPoints.Item1, controlPoints.Item2, end);
        }

        private void DrawRelationEndpoints(Graphics g, Point start, Point end, Color color)
        {
            switch (_relationshipModel.RelationType)
            {
                case "1-1":
                    DrawOneToOneEndpoints(g, start, end, color);
                    break;

                case "1-n":
                    DrawOneToManyEndpoints(g, start, end, color);
                    break;

                case "n-m":
                    DrawManyToManyEndpoints(g, start, end, color);
                    break;
            }
        }

        private void DrawOneToOneEndpoints(Graphics g, Point start, Point end, Color color)
        {
            var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
            using var pen = new Pen(color, LINE_THICKNESS);

            // 시작점 수직선
            DrawVerticalLine(g, start, angle, pen);

            // 끝점 수직선
            DrawVerticalLine(g, end, angle + Math.PI, pen);
        }

        private void DrawOneToManyEndpoints(Graphics g, Point start, Point end, Color color)
        {
            var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
            using var pen = new Pen(color, LINE_THICKNESS);

            // 시작점 수직선
            DrawVerticalLine(g, start, angle, pen);

            // 끝점 까마귀 발
            DrawCrowsFoot(g, end, angle + Math.PI, pen);
        }

        private void DrawManyToManyEndpoints(Graphics g, Point start, Point end, Color color)
        {
            var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
            using var pen = new Pen(color, LINE_THICKNESS);

            // 양쪽 까마귀 발
            DrawCrowsFoot(g, start, angle, pen);
            DrawCrowsFoot(g, end, angle + Math.PI, pen);
        }

        private void DrawVerticalLine(Graphics g, Point point, double angle, Pen pen)
        {
            var perpendicular = angle + Math.PI / 2;
            var length = ARROW_SIZE;

            g.DrawLine(pen,
                (float)(point.X - Math.Cos(perpendicular) * length),
                (float)(point.Y - Math.Sin(perpendicular) * length),
                (float)(point.X + Math.Cos(perpendicular) * length),
                (float)(point.Y + Math.Sin(perpendicular) * length));
        }

        private void DrawCrowsFoot(Graphics g, Point point, double angle, Pen pen)
        {
            var length = ARROW_SIZE;
            var width = ARROW_SIZE / 2;

            var center = new Point(
                (int)(point.X - Math.Cos(angle) * length),
                (int)(point.Y - Math.Sin(angle) * length));

            var perpendicular = angle + Math.PI / 2;

            var top = new Point(
                (int)(center.X + Math.Cos(perpendicular) * width),
                (int)(center.Y + Math.Sin(perpendicular) * width));

            var bottom = new Point(
                (int)(center.X - Math.Cos(perpendicular) * width),
                (int)(center.Y - Math.Sin(perpendicular) * width));

            g.DrawLine(pen, point, top);
            g.DrawLine(pen, point, bottom);
            g.DrawLine(pen, top, bottom);
        }

        private void DrawRelationLabel(Graphics g, Point start, Point end)
        {
            if (string.IsNullOrEmpty(_relationshipModel.Name))
                return;

            var midPoint = new Point(
                (start.X + end.X) / 2,
                (start.Y + end.Y) / 2);

            using var font = new Font("Arial", 8);
            var size = g.MeasureString(_relationshipModel.Name, font);
            var rect = new RectangleF(
                midPoint.X - size.Width / 2,
                midPoint.Y - size.Height / 2,
                size.Width,
                size.Height);

            if (_isSelected || _isHovered)
            {
                using var brush = new SolidBrush(Color.FromArgb(240, 240, 240));
                g.FillRectangle(brush, rect);
            }

            using var textBrush = new SolidBrush(GetLineColor());
            g.DrawString(_relationshipModel.Name, font, textBrush, rect);
        }

        private Color GetLineColor()
        {
            if (_isSelected)
                return Color.Blue;
            if (_isHovered)
                return Color.DodgerBlue;
            return Color.Gray;
        }

        private DashStyle GetLineStyle()
        {
            return _relationshipModel.RelationType switch
            {
                "1-1" => DashStyle.Solid,
                "1-n" => DashStyle.Solid,
                "n-m" => DashStyle.Dash,
                _ => DashStyle.Solid
            };
        }

        private (Point start, Point end) CalculateConnectionPoints()
        {
            var sourceCenter = new Point(
                _sourceTable.Left + _sourceTable.Width / 2,
                _sourceTable.Top + _sourceTable.Height / 2);

            var targetCenter = new Point(
                _targetTable.Left + _targetTable.Width / 2,
                _targetTable.Top + _targetTable.Height / 2);

            return (
                GetIntersectionPoint(_sourceTable, sourceCenter, targetCenter),
                GetIntersectionPoint(_targetTable, targetCenter, sourceCenter)
            );
        }

        private Point GetIntersectionPoint(Control table, Point start, Point end)
        {
            var rect = new Rectangle(table.Left, table.Top, table.Width, table.Height);
            var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);

            // 테이블 경계와의 교차점 계산
            if (Math.Abs(Math.Cos(angle)) > Math.Abs(Math.Sin(angle)))
            {
                // 수평 경계와의 교차
                var x = Math.Cos(angle) > 0 ? rect.Right : rect.Left;
                var y = start.Y + (x - start.X) * Math.Tan(angle);
                return new Point((int)x, (int)y);
            }
            else
            {
                // 수직 경계와의 교차
                var y = Math.Sin(angle) > 0 ? rect.Bottom : rect.Top;
                var x = start.X + (y - start.Y) / Math.Tan(angle);
                return new Point((int)x, (int)y);
            }
        }

        private (Point, Point) CalculateControlPoints(Point start, Point end)
        {
            var distance = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
            var controlDistance = distance / 3;

            var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);

            return (
                new Point(
                    (int)(start.X + Math.Cos(angle) * controlDistance),
                    (int)(start.Y + Math.Sin(angle) * controlDistance)),
                new Point(
                    (int)(end.X - Math.Cos(angle) * controlDistance),
                    (int)(end.Y - Math.Sin(angle) * controlDistance))
            );
        }

        private void UpdateHitArea(Point start, Point end)
        {
            var left = Math.Min(start.X, end.X) - HIT_AREA_WIDTH;
            var top = Math.Min(start.Y, end.Y) - HIT_AREA_WIDTH;
            var right = Math.Max(start.X, end.X) + HIT_AREA_WIDTH;
            var bottom = Math.Max(start.Y, end.Y) + HIT_AREA_WIDTH;

            _hitArea = Rectangle.FromLTRB(left, top, right, bottom);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var isInHitArea = _hitArea.Contains(e.Location);
            if (_isHovered != isInHitArea)
            {
                _isHovered = isInHitArea;
                Invalidate();
            }
        }

        private void OnRelationMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _hitArea.Contains(e.Location))
            {
                IsSelected = true;
                RelationSelected?.Invoke(this, new RelationEventArgs(_relationshipModel));
            }
        }

        private void OnRelationMouseEnter(object sender, EventArgs e)
        {
            _isHovered = true;
            Invalidate();
        }

        private void OnRelationMouseLeave(object sender, EventArgs e)
        {
            _isHovered = false;
            Invalidate();
        }

        private void OnRelationPaint(object sender, PaintEventArgs e)
        {
            // 이미 OnPaint에서 처리됨
        }

        private void OnTableLocationChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void OnTableSizeChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void OnEditRelation(object sender, EventArgs e)
        {
            try
            {
                using var dialog = new RelationshipEditorForm(_relationshipModel);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    UpdateRelationship(dialog.RelationshipModel);
                    RelationModified?.Invoke(this, new RelationEventArgs(_relationshipModel));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"관계 편집 중 오류가 발생했습니다: {ex.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ChangeRelationType(string type)
        {
            _relationshipModel.RelationType = type;
            Invalidate();
            RelationModified?.Invoke(this, new RelationEventArgs(_relationshipModel));
        }

        private void OnDeleteRelation(object sender, EventArgs


            {
            Relationship = relationship ?? throw new ArgumentNullException(nameof(relationship));
            _sourceTable = sourceTable ?? throw new ArgumentNullException(nameof(sourceTable));
            _targetTable = targetTable ?? throw new ArgumentNullException(nameof(targetTable));

            SetStyle(ControlStyles.SupportsTransparentBackColor |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint, true);

            BackColor = Color.Transparent;
            InitializeContextMenu();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var pen = new Pen(_isSelected ? Color.Blue : Color.Black, _isSelected ? 2f : 1f);
            ConfigurePenStyle(pen);

            Point start = GetConnectionPoint(_sourceTable, _targetTable);
            Point end = GetConnectionPoint(_targetTable, _sourceTable);

            // 관계선 그리기
            e.Graphics.DrawLine(pen, start, end);

            // 관계 표시 심볼 그리기
            DrawRelationSymbols(e.Graphics, start, end);

            // 관계 타입 텍스트 그리기
            DrawRelationText(e.Graphics, start, end);
        }

        private void ConfigurePenStyle(Pen pen)
        {
            switch (Relationship.RelationType)
            {
                case "1-1":
                    pen.DashStyle = DashStyle.Solid;
                    break;
                case "1-n":
                    pen.DashStyle = DashStyle.Dash;
                    break;
                case "n-m":
                    pen.DashStyle = DashStyle.Dot;
                    break;
            }

            pen.CustomEndCap = new AdjustableArrowCap(6, 6);
        }

        private Point GetConnectionPoint(ERDTableControl source, ERDTableControl target)
        {
            Point sourceCenter = new Point(
                source.Left + source.Width / 2,
                source.Top + source.Height / 2);
            Point targetCenter = new Point(
                target.Left + target.Width / 2,
                target.Top + target.Height / 2);

            // 두 테이블 사이의 각도 계산
            double angle = Math.Atan2(targetCenter.Y - sourceCenter.Y,
                                    targetCenter.X - sourceCenter.X);

            // 테이블 경계에서의 교차점 계산
            return new Point(
                (int)(sourceCenter.X + (source.Width / 2) * Math.Cos(angle)),
                (int)(sourceCenter.Y + (source.Height / 2) * Math.Sin(angle))
            );
        }

        private void DrawRelationSymbols(Graphics g, Point start, Point end)
        {
            switch (Relationship.RelationType)
            {
                case "1-1":
                    DrawOneToOneSymbols(g, start, end);
                    break;
                case "1-n":
                    DrawOneToManySymbols(g, start, end);
                    break;
                case "n-m":
                    DrawManyToManySymbols(g, start, end);
                    break;
            }
        }

        private void DrawOneToOneSymbols(Graphics g, Point start, Point end)
        {
            using var pen = new Pen(Color.Black, 1);

            // 시작점 심볼
            DrawVerticalLine(g, pen, start, 6);

            // 끝점 심볼
            DrawVerticalLine(g, pen, end, 6);
        }

        private void DrawOneToManySymbols(Graphics g, Point start, Point end)
        {
            using var pen = new Pen(Color.Black, 1);

            // 시작점 심볼 (One)
            DrawVerticalLine(g, pen, start, 6);

            // 끝점 심볼 (Many)
            DrawCrowFoot(g, pen, end);
        }

        private void DrawManyToManySymbols(Graphics g, Point start, Point end)
        {
            using var pen = new Pen(Color.Black, 1);

            // 양쪽 끝에 Crow's foot 심볼
            DrawCrowFoot(g, pen, start);
            DrawCrowFoot(g, pen, end);
        }

        private void DrawVerticalLine(Graphics g, Pen pen, Point point, int length)
        {
            g.DrawLine(pen,
                      point.X, point.Y - length,
                      point.X, point.Y + length);
        }

        private void DrawCrowFoot(Graphics g, Pen pen, Point point)
        {
            // 중앙선
            g.DrawLine(pen,
                      point.X - 10, point.Y,
                      point.X + 10, point.Y);

            // 위쪽 대각선
            g.DrawLine(pen,
                      point.X, point.Y,
                      point.X + 10, point.Y - 6);

            // 아래쪽 대각선
            g.DrawLine(pen,
                      point.X, point.Y,
                      point.X + 10, point.Y + 6);
        }

        private void DrawRelationText(Graphics g, Point start, Point end)
        {
            Point textPoint = new Point(
                (start.X + end.X) / 2,
                (start.Y + end.Y) / 2
            );

            string text = GetRelationText();
            using var font = new Font("Arial", 8);
            using var brush = new SolidBrush(Color.Black);

            var size = g.MeasureString(text, font);
            textPoint.X -= (int)(size.Width / 2);
            textPoint.Y -= (int)(size.Height / 2);

            using var path = new GraphicsPath();
            path.AddString(text,
                         font.FontFamily,
                         (int)font.Style,
                         font.Size,
                         textPoint,
                         StringFormat.GenericDefault);

            if (_isSelected)
            {
                using var shadowBrush = new SolidBrush(Color.White);
                g.FillPath(shadowBrush, path);
            }

            g.DrawPath(new Pen(Color.Black, 0.5f), path);
            g.FillPath(brush, path);
        }

        private string GetRelationText()
        {
            return Relationship.RelationType switch
            {
                "1-1" => "1:1",
                "1-n" => "1:N",
                "n-m" => "N:M",
                _ => "?"
            };
        }

        private void InitializeContextMenu()
        {
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("관계 유형 변경(&T)", null, null, new ToolStripMenuItem[]
                {
                    new ToolStripMenuItem("1:1", null, (s, e) => ChangeRelationType("1-1")),
                    new ToolStripMenuItem("1:N", null, (s, e) => ChangeRelationType("1-n")),
                    new ToolStripMenuItem("N:M", null, (s, e) => ChangeRelationType("n-m"))
                }),
                new ToolStripMenuItem("관계 삭제(&D)", null, OnDeleteRelation)
            });

            ContextMenuStrip = contextMenu;
        }

        private void ChangeRelationType(string newType)
        {
            if (Relationship.RelationType != newType)
            {
                Relationship.RelationType = newType;
                Invalidate();
                Parent?.Invalidate();
            }
        }

        private void OnDeleteRelation(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "이 관계를 삭제하시겠습니까?",
                "관계 삭제",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Parent.Controls.Remove(this);
                Parent.Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                IsSelected = true;
                RelationSelected?.Invoke(this, new RelationEventArgs(Relationship));

                // 다른 관계선들의 선택 상태 해제
                foreach (Control control in Parent.Controls)
                {
                    if (control is ERDRelationControl relationControl && control != this)
                    {
                        relationControl.IsSelected = false;
                    }
                }
            }
        }
    }

    public class RelationEventArgs : EventArgs
    {
        public RelationshipModel Relationship { get; }

        public RelationEventArgs(RelationshipModel relationship)
        {
            Relationship = relationship;
        }
    }
}