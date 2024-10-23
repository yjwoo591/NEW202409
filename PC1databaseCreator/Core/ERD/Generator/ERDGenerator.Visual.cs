using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;
using PC1MAINAITradingSystem.Models.ERD;
using System.ComponentModel.DataAnnotations;

namespace PC1MAINAITradingSystem.Core.ERD.Generator
{
    public class VisualGenerator : BaseERDGenerator
    {
        private const int TABLE_PADDING = 20;
        private const int TABLE_MARGIN = 50;
        private const int COLUMN_HEIGHT = 25;
        private const int MIN_TABLE_WIDTH = 200;
        private const int TITLE_HEIGHT = 40;

        public VisualGenerator(ILogger logger) : base(logger) { }

        public override async Task<ValidationResult> Validate(ERDModel model)
        {
            var result = await ValidateBasic(model);
            if (!result.IsValid)
            {
                return result;
            }

            // 시각화 특정 검증
            foreach (var table in model.Tables)
            {
                // 테이블 이름 길이 검증
                if (table.Name.Length > 30)
                {
                    result.AddWarning($"Table name '{table.Name}' is too long for optimal visualization");
                }

                // 컬럼 개수 검증
                if (table.Columns.Count > 20)
                {
                    result.AddWarning($"Table '{table.Name}' has too many columns for clear visualization");
                }

                foreach (var column in table.Columns)
                {
                    // 컬럼 이름 길이 검증
                    if (column.Name.Length > 30)
                    {
                        result.AddWarning($"Column name '{column.Name}' in table '{table.Name}' is too long for optimal visualization");
                    }
                }
            }

            return result;
        }

        public override async Task<(string Content, List<string> Warnings)> Generate(ERDModel model, GenerationOptions options)
        {
            try
            {
                var warnings = new List<string>();
                var layout = CalculateLayout(model);
                var svg = new StringBuilder();

                // SVG 헤더 생성
                GenerateSvgHeader(svg, layout.Width, layout.Height);

                // 정의 섹션 생성
                GenerateDefinitions(svg);

                // 테이블 그리기
                foreach (var tableLayout in layout.Tables)
                {
                    await GenerateTable(svg, tableLayout, options);
                }

                // 관계선 그리기
                foreach (var relationship in model.Relationships)
                {
                    await GenerateRelationship(svg, relationship, layout, warnings);
                }

                // SVG 푸터 생성
                svg.AppendLine("</svg>");

                await Logger.LogInfo("Generated visual ERD diagram");
                return (svg.ToString(), warnings);
            }
            catch (Exception ex)
            {
                await Logger.LogError("Failed to generate visual ERD", ex);
                throw;
            }
        }

        private ERDLayout CalculateLayout(ERDModel model)
        {
            var layout = new ERDLayout();
            var currentX = TABLE_MARGIN;
            var currentY = TABLE_MARGIN;
            var maxRowHeight = 0;
            var maxWidth = 0;

            foreach (var table in model.Tables)
            {
                var tableWidth = CalculateTableWidth(table);
                var tableHeight = CalculateTableHeight(table);

                // 새 행이 필요한지 확인
                if (currentX + tableWidth + TABLE_MARGIN > 2000) // 최대 너비 제한
                {
                    currentX = TABLE_MARGIN;
                    currentY += maxRowHeight + TABLE_MARGIN;
                    maxRowHeight = 0;
                }

                var tableLayout = new TableLayout
                {
                    Table = table,
                    X = currentX,
                    Y = currentY,
                    Width = tableWidth,
                    Height = tableHeight
                };

                layout.Tables.Add(tableLayout);

                currentX += tableWidth + TABLE_MARGIN;
                maxRowHeight = Math.Max(maxRowHeight, tableHeight);
                maxWidth = Math.Max(maxWidth, currentX);
            }

            layout.Width = maxWidth + TABLE_MARGIN;
            layout.Height = currentY + maxRowHeight + TABLE_MARGIN;

            return layout;
        }

        private void GenerateSvgHeader(StringBuilder svg, int width, int height)
        {
            svg.AppendLine($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<svg width=""{width}"" height=""{height}"" version=""1.1"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"">");
        }

        private void GenerateDefinitions(StringBuilder svg)
        {
            svg.AppendLine(@"<defs>
    <marker id=""arrow"" viewBox=""0 0 10 10"" refX=""9"" refY=""5""
            markerWidth=""6"" markerHeight=""6"" orient=""auto"">
        <path d=""M 0 0 L 10 5 L 0 10 z"" fill=""#666""/>
    </marker>
    <marker id=""diamond"" viewBox=""0 0 10 10"" refX=""9"" refY=""5""
            markerWidth=""6"" markerHeight=""6"" orient=""auto"">
        <path d=""M 0 5 L 5 10 L 10 5 L 5 0 z"" fill=""#666""/>
    </marker>
</defs>");
        }

        private async Task GenerateTable(StringBuilder svg, TableLayout layout, GenerationOptions options)
        {
            var table = layout.Table;

            // 테이블 그룹 시작
            svg.AppendLine($@"<g class=""table"" transform=""translate({layout.X},{layout.Y})"">
    <!-- Table Background -->
    <rect x=""0"" y=""0"" width=""{layout.Width}"" height=""{layout.Height}""
          fill=""white"" stroke=""#666"" stroke-width=""2"" rx=""5""/>");

            // 테이블 헤더
            svg.AppendLine($@"    <!-- Table Header -->
    <rect x=""0"" y=""0"" width=""{layout.Width}"" height=""{TITLE_HEIGHT}""
          fill=""#4a9eff"" stroke=""none"" rx=""5""/>");

            // 테이블 이름
            svg.AppendLine($@"    <text x=""{layout.Width / 2}"" y=""{TITLE_HEIGHT / 2}""
          font-family=""Arial"" font-size=""16"" font-weight=""bold""
          text-anchor=""middle"" dominant-baseline=""middle"" fill=""white"">
        {table.Name}
    </text>");

            // 컬럼들
            var yOffset = TITLE_HEIGHT;
            foreach (var column in table.Columns)
            {
                await GenerateColumn(svg, column, yOffset, layout.Width);
                yOffset += COLUMN_HEIGHT;
            }

            // 테이블 그룹 종료
            svg.AppendLine("</g>");
        }

        private async Task GenerateColumn(StringBuilder svg, ColumnModel column, int yOffset, int tableWidth)
        {
            var icons = new StringBuilder();
            if (column.IsPrimaryKey)
                icons.Append("🔑 ");
            if (column.IsForeignKey)
                icons.Append("🔗 ");

            svg.AppendLine($@"    <!-- Column: {column.Name} -->
    <rect x=""5"" y=""{yOffset}"" width=""{tableWidth - 10}"" height=""{COLUMN_HEIGHT}""
          fill=""#f8f9fa"" stroke=""#ddd"" stroke-width=""1""/>");

            svg.AppendLine($@"    <text x=""10"" y=""{yOffset + COLUMN_HEIGHT / 2}""
          font-family=""Arial"" font-size=""14""
          dominant-baseline=""middle"" fill=""#333"">
        {icons}{column.Name}: {GetDataTypeString(column)}
    </text>");
        }

        private async Task GenerateRelationship(StringBuilder svg, RelationshipModel relationship,
            ERDLayout layout, List<string> warnings)
        {
            var sourceTable = layout.Tables.FirstOrDefault(t => t.Table.Name == relationship.SourceTable);
            var targetTable = layout.Tables.FirstOrDefault(t => t.Table.Name == relationship.TargetTable);

            if (sourceTable == null || targetTable == null)
            {
                warnings.Add($"Cannot draw relationship between {relationship.SourceTable} and {relationship.TargetTable}");
                return;
            }

            var (startX, startY) = GetConnectionPoint(sourceTable, targetTable, true);
            var (endX, endY) = GetConnectionPoint(targetTable, sourceTable, false);

            var marker = relationship.RelationType switch
            {
                "1-1" => "marker-end=\"url(#arrow)\"",
                "1-n" => "marker-end=\"url(#arrow)\"",
                "n-m" => "marker-end=\"url(#diamond)\"",
                _ => string.Empty
            };

            svg.AppendLine($@"    <!-- Relationship: {relationship.SourceTable} -> {relationship.TargetTable} -->
    <path d=""M {startX} {startY} L {endX} {endY}""
          fill=""none"" stroke=""#666"" stroke-width=""2"" {marker}/>");
        }

        private (int X, int Y) GetConnectionPoint(TableLayout source, TableLayout target, bool isSource)
        {
            var sourceCenter = new Point(
                source.X + source.Width / 2,
                source.Y + source.Height / 2);
            var targetCenter = new Point(
                target.X + target.Width / 2,
                target.Y + target.Height / 2);

            var angle = Math.Atan2(targetCenter.Y - sourceCenter.Y, targetCenter.X - sourceCenter.X);
            var box = isSource ? source : target;

            // 테이블 경계와의 교차점 계산
            if (Math.Abs(Math.Cos(angle)) > Math.Abs(Math.Sin(angle)))
            {
                var x = Math.Cos(angle) > 0 ? box.X + box.Width : box.X;
                var y = box.Y + box.Height / 2 + Math.Tan(angle) * (x - (box.X + box.Width / 2));
                return ((int)x, (int)y);
            }
            else
            {
                var y = Math.Sin(angle) > 0 ? box.Y + box.Height : box.Y;
                var x = box.X + box.Width / 2 + (y - (box.Y + box.Height / 2)) / Math.Tan(angle);
                return ((int)x, (int)y);
            }
        }

        private int CalculateTableWidth(TableModel table)
        {
            // 테이블 이름과 각 컬럼의 예상 너비를 계산
            var maxWidth = Math.Max(
                MeasureTextWidth(table.Name) + TABLE_PADDING * 2,
                table.Columns.Max(c => MeasureTextWidth($"{c.Name}: {GetDataTypeString(c)}")) + TABLE_PADDING * 2
            );

            return Math.Max(maxWidth, MIN_TABLE_WIDTH);
        }

        private int CalculateTableHeight(TableModel table)
        {
            return TITLE_HEIGHT + (table.Columns.Count * COLUMN_HEIGHT);
        }

        private int MeasureTextWidth(string text)
        {
            // 간단한 텍스트 너비 추정 (실제로는 폰트 메트릭스를 사용해야 함)
            return text.Length * 8;
        }
    }

    internal class ERDLayout
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<TableLayout> Tables { get; set; } = new();
    }

    internal class TableLayout
    {
        public TableModel Table { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class VisualGenerationOptions : GenerationOptions
    {
        public string BackgroundColor { get; set; } = "#ffffff";
        public string TableHeaderColor { get; set; } = "#4a9eff";
        public string TableBorderColor { get; set; } = "#666666";
        public string TextColor { get; set; } = "#333333";
        public string RelationshipColor { get; set; } = "#666666";
        public bool ShowColumnTypes { get; set; } = true;
        public bool ShowConstraints { get; set; } = true;
        public int MaxDiagramWidth { get; set; } = 2000;
        public bool AutoLayout { get; set; } = true;
        public Dictionary<string, Point> TablePositions { get; set; } = new();
    }
}