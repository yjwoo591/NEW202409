using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Principal;

namespace PC1databaseCreator.Core.ERD
{
    /// <summary>
    /// ERD 기반 SQL 스크립트 생성을 담당하는 클래스
    /// </summary>
    public class ERDGenerator
    {
        #region Constants
        private const string CREATE_TABLE_TEMPLATE = "CREATE TABLE {0} (\n{1}\n)";
        private const string FOREIGN_KEY_TEMPLATE =
            "ALTER TABLE {0} ADD CONSTRAINT FK_{0}_{1} FOREIGN KEY ({2}) REFERENCES {1}({3})";
        #endregion

        #region Fields
        private readonly Dictionary<string, string> _dataTypeMap;
        #endregion

        #region Constructor
        public ERDGenerator()
        {
            _dataTypeMap = InitializeDataTypeMap();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 엔티티 목록으로부터 CREATE TABLE 스크립트를 생성합니다.
        /// </summary>
        /// <param name="entities">엔티티 목록</param>
        /// <returns>생성된 SQL 스크립트</returns>
        public string GenerateCreateTableScript(List<ERDEntity> entities)
        {
            var scriptBuilder = new StringBuilder();

            foreach (var entity in entities)
            {
                scriptBuilder.AppendLine(GenerateTableScript(entity));
                scriptBuilder.AppendLine("GO");
                scriptBuilder.AppendLine();
            }

            foreach (var entity in entities)
            {
                if (entity.Relationships != null)
                {
                    foreach (var relationship in entity.Relationships)
                    {
                        scriptBuilder.AppendLine(GenerateForeignKeyScript(relationship));
                        scriptBuilder.AppendLine("GO");
                        scriptBuilder.AppendLine();
                    }
                }
            }

            return scriptBuilder.ToString();
        }
        #endregion

        #region Private Methods
        private Dictionary<string, string> InitializeDataTypeMap()
        {
            return new Dictionary<string, string>
            {
                { "string", "NVARCHAR" },
                { "int", "INT" },
                { "datetime", "DATETIME" },
                { "bool", "BIT" },
                { "decimal", "DECIMAL" },
                { "date", "DATE" }
            };
        }

        private string GenerateTableScript(ERDEntity entity)
        {
            var columnDefinitions = new StringBuilder();

            foreach (var column in entity.Columns)
            {
                if (columnDefinitions.Length > 0)
                    columnDefinitions.AppendLine(",");

                columnDefinitions.Append($"    {GenerateColumnDefinition(column)}");
            }

            return string.Format(CREATE_TABLE_TEMPLATE, entity.Name, columnDefinitions.ToString());
        }

        private string GenerateColumnDefinition(ERDColumn column)
        {
            var definition = new StringBuilder();

            definition.Append($"{column.Name} {MapDataType(column.DataType)}");

            if (!string.IsNullOrEmpty(column.Constraints))
            {
                if (column.Constraints.Contains("PK"))
                    definition.Append(" PRIMARY KEY");
                if (column.Constraints.Contains("NOT NULL"))
                    definition.Append(" NOT NULL");
                if (column.Constraints.Contains("UNIQUE"))
                    definition.Append(" UNIQUE");
            }

            return definition.ToString();
        }

        private string MapDataType(string sourceType)
        {
            var typeInfo = sourceType.Split('(');
            var baseType = typeInfo[0].ToLower();

            if (_dataTypeMap.TryGetValue(baseType, out string sqlType))
            {
                if (typeInfo.Length > 1)
                    return $"{sqlType}({typeInfo[1]}";
                return sqlType;
            }

            return sourceType.ToUpper();
        }

        private string GenerateForeignKeyScript(ERDRelationship relationship)
        {
            // FK 컬럼명은 참조테이블명 + Id 규칙 사용
            string fkColumnName = $"{relationship.ToEntity}Id";
            string pkColumnName = "Id"; // PK 컬럼명은 Id로 가정

            return string.Format(FOREIGN_KEY_TEMPLATE,
                relationship.FromEntity,  // FK를 가지는 테이블
                relationship.ToEntity,    // 참조되는 테이블
                fkColumnName,            // FK 컬럼명
                pkColumnName);           // 참조되는 PK 컬럼명
        }
        #endregion
    }
}