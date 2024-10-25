using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PC1databaseCreator.Core.ERD
{
    /// <summary>
    /// Mermaid ERD 파일을 파싱하는 클래스
    /// </summary>
    public class ERDParser
    {
        #region Constants
        private const string ENTITY_PATTERN = @"(\w+)\s*{([^}]*)}";
        private const string COLUMN_PATTERN = @"\s*(\w+)\s+([\w\(\)]+)\s*(\w+)?";
        private const string RELATIONSHIP_PATTERN = @"(\w+)\s*([|\-o<>}{\]|\[])+\s*(\w+)";
        #endregion

        #region Fields
        private readonly Encoding _encoding;
        private readonly Dictionary<string, ERDEntity> _entityMap;
        #endregion

        #region Constructor
        /// <summary>
        /// ERDParser 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="encoding">파일 인코딩 (기본값: UTF8)</param>
        public ERDParser(Encoding encoding = null)
        {
            _encoding = encoding ?? Encoding.UTF8;
            _entityMap = new Dictionary<string, ERDEntity>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// ERD 파일을 파싱하여 엔티티 목록을 반환합니다.
        /// </summary>
        /// <param name="content">ERD 파일 내용</param>
        /// <returns>파싱된 엔티티 목록</returns>
        /// <exception cref="InvalidERDFormatException">ERD 형식이 잘못된 경우</exception>
        public List<ERDEntity> Parse(string content)
        {
            try
            {
                _entityMap.Clear();
                ParseEntities(content);
                ParseRelationships(content);
                return new List<ERDEntity>(_entityMap.Values);
            }
            catch (Exception ex)
            {
                throw new InvalidERDFormatException("ERD 파싱 중 오류가 발생했습니다.", ex);
            }
        }
        #endregion

        #region Private Methods
        private void ParseEntities(string content)
        {
            var matches = Regex.Matches(content, ENTITY_PATTERN, RegexOptions.Multiline);
            foreach (Match match in matches)
            {
                string entityName = match.Groups[1].Value.Trim();
                string columnsContent = match.Groups[2].Value;

                var entity = new ERDEntity { Name = entityName };
                ParseColumns(columnsContent, entity);
                _entityMap[entityName] = entity;
            }
        }

        private void ParseColumns(string columnsContent, ERDEntity entity)
        {
            var matches = Regex.Matches(columnsContent, COLUMN_PATTERN, RegexOptions.Multiline);
            foreach (Match match in matches)
            {
                var column = new ERDColumn
                {
                    Name = match.Groups[1].Value.Trim(),
                    DataType = match.Groups[2].Value.Trim(),
                    Constraints = match.Groups[3].Value.Trim()
                };
                entity.Columns.Add(column);
            }
        }

        private void ParseRelationships(string content)
        {
            var matches = Regex.Matches(content, RELATIONSHIP_PATTERN, RegexOptions.Multiline);
            foreach (Match match in matches)
            {
                string entity1 = match.Groups[1].Value.Trim();
                string entity2 = match.Groups[3].Value.Trim();
                string relationship = match.Groups[2].Value.Trim();

                if (_entityMap.ContainsKey(entity1) && _entityMap.ContainsKey(entity2))
                {
                    _entityMap[entity1].Relationships ??= new List<ERDRelationship>();
                    _entityMap[entity1].Relationships.Add(new ERDRelationship
                    {
                        FromEntity = entity1,
                        ToEntity = entity2,
                        Type = ParseRelationshipType(relationship)
                    });
                }
            }
        }

        private RelationshipType ParseRelationshipType(string relationship)
        {
            // 관계 타입 문자열을 RelationshipType 열거형으로 변환
            return relationship switch
            {
                "||--o{" => RelationshipType.OneToMany,
                "||--|{" => RelationshipType.OneToMany_Required,
                "||--||" => RelationshipType.OneToOne,
                "}|--|{" => RelationshipType.ManyToMany,
                _ => RelationshipType.Unknown
            };
        }
        #endregion
    }

    /// <summary>
    /// ERD 관계 타입을 정의하는 열거형
    /// </summary>
    public enum RelationshipType
    {
        Unknown,
        OneToOne,
        OneToMany,
        OneToMany_Required,
        ManyToMany
    }

    /// <summary>
    /// ERD 엔티티 간의 관계를 정의하는 클래스
    /// </summary>
    public class ERDRelationship
    {
        /// <summary>
        /// 관계의 시작 엔티티
        /// </summary>
        public string FromEntity { get; set; }

        /// <summary>
        /// 관계의 대상 엔티티
        /// </summary>
        public string ToEntity { get; set; }

        /// <summary>
        /// 관계 타입
        /// </summary>
        public RelationshipType Type { get; set; }
    }
}