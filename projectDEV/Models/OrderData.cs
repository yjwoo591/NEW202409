using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForexAITradingPC1Main.Models
{
    public class OrderData
    {
        [Key]
        public int Id { get; set; }

        public int SeriesId { get; set; }
        [ForeignKey("SeriesId")]
        public virtual Series Series { get; set; }

        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }

        [Required]
        [StringLength(8)]
        public string TimeH { get; set; }

        [Required]
        [StringLength(6)]
        public string Time { get; set; }

        [Required]
        [StringLength(9)]
        public string TimeS { get; set; }

        public int SequenceNumber { get; set; }

        [Required]
        public DateTime DT { get; set; }

        [Required]
        [StringLength(50)]
        public string ContractMonth { get; set; }

        [StringLength(10)]
        public string Test { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; }

        public int OrderQuantity { get; set; }
        public int RemainingQuantity { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal OrderPrice { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal ConditionPrice { get; set; }

        [Required]
        [StringLength(10)]
        public string BuySellType { get; set; }

        [Required]
        [StringLength(20)]
        public string PriceType { get; set; }

        [Required]
        [StringLength(20)]
        public string ExecutionType { get; set; }

        [StringLength(20)]
        public string ConditionType { get; set; }

        [StringLength(10)]
        public string HTSType { get; set; }

        [StringLength(50)]
        public string UserID { get; set; }

        [StringLength(20)]
        public string FullExecutionType { get; set; }

        [Required]
        [StringLength(50)]
        public string PartitionKey { get; set; }
    }
}



/*
 *
 *
 *네, 이 오류는 OrderData 모델 클래스에 TimeS 속성이 정의되어 있지 않아 생성된 것입니다. OrderData 모델을 수정하여 이 문제를 해결해 보세요.
OrderData.cs 파일의 전체 내용을 다음과 같이 업데이트하겠습니다:
모델/OrderData.cs코드를 열려면 클릭하세요
업데이트된 OrderData 클래스에는 다음과 같은 변경 사항이 포함되어 있습니다:

TimeS 속성이 추가되었습니다:
씨샤프복사[Required]
[StringLength(9)]
public string TimeS { get; set; }

다른 시간 관련 속성들(TimeH, Time)과 함께 TimeS도 포함되어 있습니다.
모든 속성에 대해 적절한 데이터 조사를 사용하여 조사하고 데이터베이스를 매핑했습니다.

이제 ForexDbContext.cs 파일의 OnModelCreating 메서드에서 OrderData 구성 구성에 TimeS 속성이 포함되어 있는지 확인해야 합니다. ForexDbContext.cs 파일의 해당 부분이 다음과 같이 되도록 해주세요:
씨샤프복사modelBuilder.Entity<OrderData>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.TimeH).IsRequired().HasMaxLength(8);
    entity.Property(e => e.Time).IsRequired().HasMaxLength(6);
    entity.Property(e => e.TimeS).IsRequired().HasMaxLength(9);
    // ... 다른 속성 구성 ...
});
이러한 변경 사항을 적용한 후:

솔루션을 정리(Clean)합니다.
프로젝트를 다시 빌드합니다.

따라서 'OrderData'에 'TimeS'가 남아 있어야 합니다. 만약 아직도 문제가 발생한다면, 전체 오류 메시지와 ForexDbContext.cs 파일의 전체 내용을 공유할 수 있습니다.
*/