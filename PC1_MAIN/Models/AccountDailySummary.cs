using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForexAITradingPC1Main.Models
{
    public class AccountDailySummary
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }

        [Required]
        public DateTime SummaryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ClosingBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetDailyPnL { get; set; }

        public int TotalTrades { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalVolume { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCommissionFees { get; set; }

        [Required]
        [StringLength(50)]
        public string PartitionKey { get; set; }
    }
}

/*이 두 클래스는 프로젝트에서 구현하기로 결정했다면 Models 폴더에 추가할 수 있습니다. 이들은 거래 실적과 일일 계정 요약을 기록하기 위한 구조를 제공하며, 이는 Forex AI 거래 시스템에서 분석 및 보고에 매우 유용할 수 있습니다.
이 모델을 사용하려면:

모델 폴더에 다음 파일을 만듭니다.
ForexDbContext.cs 파일에 해당 DbSet 속성을 추가합니다.
데이터베이스 스키마를 업데이트하거나(마이그레이션을 사용하는 경우) 데이터베이스에 해당 테이블을 만듭니다.
거래 및 데이터 처리 논리에 이러한 엔터티를 채우는 논리를 구현합니다.
*/