using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using PC1MAINAITradingSystem.Forms.Controls;
using System.Drawing.Drawing2D;

namespace PC1MAINAITradingSystem.Forms
{
    public partial class MainForm
    {
        private const int TABLE_MARGIN = 20;
        private const int MIN_TABLE_WIDTH = 200;
        private bool _isDraggingTable;
        private Point _dragStartPoint;
        private ERDTableControl _draggedTable;

        partial void InitializeERDVisual()
        {
            _erdVisualPanel.AllowDrop = true;
            _erdVisualPanel.Paint += ERDVisualPanel_Paint;
            _erdVisualPanel.MouseDown += ERDVisualPanel_MouseDown;
            _erdVisualPanel.MouseMove += ERDVisualPanel_MouseMove;
            _erdVisualPanel.MouseUp += ERDVisualPanel_MouseUp;
        }

        private async Task UpdateVisualView()
        {
            try
            {
                if (_mainTabControl.SelectedTab.Text != "Visual View") return;

                _erdVisualPanel.SuspendLayout();
                _erdVisualPanel.Controls.Clear();

                if (_currentERD == null) return;

                // 테이블 컨트롤 생성
                foreach (var table in _currentERD.Tables)
                {
                    var tableControl = new ERDTableControl(table)
                    {
                        Location = CalculateTablePosition(table)
                    };
                    _erdVisualPanel.Controls.Add(tableControl);
                }

                // 관계선 다시 그리기
                _erdVisualPanel.Refresh();
                _erdVisualPanel.ResumeLayout();

                await _logger.Info("Visual view updated");
            }
            catch (Exception ex)
            {
                await _logger.Error("Failed to update visual view", ex);
                _erdVisualPanel.ResumeLayout();
            }
        }

        private Point CalculateTablePosition(TableModel table)
        {
            // 테이블 위치 계산 로직 구현
            int x = TABLE_MARGIN;
            int y = TABLE_MARGIN;

            // 기존 테이블들과 겹치지 않는 위치 찾기
            bool overlap;
            do
            {
                overlap = false;
                foreach (ERDTableControl existingTable in _erdVisualPanel.Controls)
                {
                    if (IsOverlapping(new Rectangle(x, y, MIN_TABLE_WIDTH, existingTable.Height),
                                    existingTable.Bounds))
                    {
                        x += TABLE_MARGIN + MIN_TABLE_WIDTH;
                        if (x + MIN_TABLE_WIDTH > _erdVisualPanel.Width)
                        {
                            x = TABLE_MARGIN;
                            y += existingTable.Height + TABLE_MARGIN;
                        }
                        overlap = true;
                        break;
                    }
                }
            } while (overlap);

            return new Point(x, y);
        }

        private bool IsOverlapping(Rectangle rect1, Rectangle rect2)
        {
            return rect1.IntersectsWith(rect2);
        }

        private void ERDVisualPanel_Paint(object sender, PaintEventArgs e)
        {
            if (_currentERD == null) return;

            using var pen = new Pen(Color.Black, 1)
            {
                CustomEndCap = new AdjustableArrowCap(6, 6)
            };

            // 관계선 그리기
            foreach (var relationship in _currentERD.Relationships)
            {
                var sourceTable = FindTableControl(relationship.SourceTable);
                var targetTable = FindTableControl(relationship.TargetTable);

                if (sourceTable == null || targetTable == null) continue;

                DrawRelationshipLine(e.Graphics, pen, sourceTable, targetTable, relationship.RelationType);
            }
        }

        private ERDTableControl FindTableControl(string tableName)
        {
            foreach (Control control in _erdVisualPanel.Controls)
            {
                if (control is ERDTableControl tableControl &&
                    tableControl.TableModel.Name == tableName)
                {
                    return tableControl;
                }
            }
            return null;
        }

        private void DrawRelationshipLine(Graphics g, Pen pen,
            ERDTableControl source, ERDTableControl target, string relationType)
        {
            // 관계 유형에 따라 선 스타일 변경
            switch (relationType)
            {
                case "1-1":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    break;
                case "1-n":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    break;
                case "n-m":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    break;
            }

            // 시작점과 끝점 계산
            Point start = CalculateConnectionPoint(source, target);
            Point end = CalculateConnectionPoint(target, source);

            g.DrawLine(pen, start, end);

            // 관계 유형 표시
            string relationText = GetRelationshipText(relationType);
            using var font = new Font("Arial", 8);
            Point textPoint = CalculateTextPoint(start, end);
            g.DrawString(relationText, font, Brushes.Black, textPoint);
        }

        private Point CalculateConnectionPoint(ERDTableControl source, ERDTableControl target)
        {
            // 두 테이블 사이의 최적 연결점 계산
            Point sourceCenter = new Point(
                source.Left + source.Width / 2,
                source.Top + source.Height / 2);
            Point targetCenter = new Point(
                target.Left + target.Width / 2,
                target.Top + target.Height / 2);

            double angle = Math.Atan2(targetCenter.Y - sourceCenter.Y,
                                    targetCenter.X - sourceCenter.X);

            return new Point(
                (int)(sourceCenter.X + (source.Width / 2) * Math.Cos(angle)),
                (int)(sourceCenter.Y + (source.Height / 2) * Math.Sin(angle))
            );
        }

        private Point CalculateTextPoint(Point start, Point end)
        {
            return new Point(
                (start.X + end.X) / 2 - 10,
                (start.Y + end.Y) / 2 - 10
            );
        }

        private string GetRelationshipText(string relationType)
        {
            return relationType switch
            {
                "1-1" => "1:1",
                "1-n" => "1:N",
                "n-m" => "N:M",
                _ => "?"
            };
        }

        private void ERDVisualPanel_MouseDown(object sender, MouseEventArgs e)
        {
            var control = _erdVisualPanel.GetChildAtPoint(e.Location);
            if (control is ERDTableControl tableControl)
            {
                _isDraggingTable = true;
                _draggedTable = tableControl;
                _dragStartPoint = e.Location;
                tableControl.BringToFront();
            }
        }

        private void ERDVisualPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingTable || _draggedTable == null) return;

            int deltaX = e.X - _dragStartPoint.X;
            int deltaY = e.Y - _dragStartPoint.Y;

            _draggedTable.Left += deltaX;
            _draggedTable.Top += deltaY;
            _dragStartPoint = e.Location;

            // 관계선 다시 그리기
            _erdVisualPanel.Refresh();
        }

        private void ERDVisualPanel_MouseUp(object sender, MouseEventArgs e)
        {
            _isDraggingTable = false;
            _draggedTable = null;
        }
    }
}