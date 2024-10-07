using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForexAITradingPC1Main.Models
{
    [Table("DealData")]
    public class DealData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SeriesId { get; set; }

        [ForeignKey("SeriesId")]
        public virtual Series Series { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        [StringLength(8)]
        public string TimeH { get; set; }

        [Required]
        [StringLength(6)]
        public string Time { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [StringLength(1)]
        public string Confirmation { get; set; }

        [Required]
        public int AccumulatedQuantity { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal AskPrice { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal BidPrice { get; set; }

        public int UnfilledQuantity { get; set; }

        [Required]
        [StringLength(1)]
        public string Sign { get; set; }

        [Required]
        [StringLength(50)]
        public string PartitionKey { get; set; }

        public DealData()
        {
            Timestamp = DateTime.UtcNow;
            TimeH = Timestamp.ToString("HHmmssff");
            Time = Timestamp.ToString("HHmmss");
            PartitionKey = Timestamp.ToString("yyyyMMdd");
        }

        public decimal GetTotalValue()
        {
            return Price * Quantity;
        }

        public bool IsBuyDeal()
        {
            return Sign == "B";
        }

        public bool IsSellDeal()
        {
            return Sign == "S";
        }

        public decimal GetSpread()
        {
            return AskPrice - BidPrice;
        }

        public bool IsFullyFilled()
        {
            return UnfilledQuantity == 0;
        }

        public decimal GetFillRate()
        {
            if (Quantity == 0) return 0;
            return (decimal)(Quantity - UnfilledQuantity) / Quantity;
        }

        public override string ToString()
        {
            return $"DealData: {Series.ToString()} at {Timestamp} - Price: {Price}, Quantity: {Quantity}, Sign: {Sign}";
        }
    }
}


/*
이 DealData.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

데이터베이스의 DealData 테이블과 매핑되는 속성을 정의합니다.
숫자는 애트리뷰트를 사용합니다.
시리즈 모델과의 관계를 활용하여 프로퍼티를 포함합니다.
생성을 설정합니다.
거래 데이터 분석에 필수적인 유용한 메서드를 포함합니다.

주요 속성:

ID: 거래 데이터의 고유한 행위입니다.
SeriesId: 닌자 시리즈의 예외입니다.
타임스탬프: 딜링 데이터의 타임스탬프입니다.
TimeH, 시간: 시간 정보입니다.
가격: 딜링 가격입니다.
수량 : 딜링 수량입니다.
확인: 거래 확인 정보입니다.
AccumulatedQuantity : 이벤트 거래 수량입니다.
AskPrice, BidPrice: 해당시점의 매도/매수호가입니다.
UnfilledQuantity: 미체결 수량입니다.
서명: 협상을 하는 서명자입니다 (B: 보이, S: 매도).
PartitionKey: 관측 키입니다 (일자별 파티셔닝을 위해 사용).

주요 방법:

GetTotalValue(): 총 거래 금액을 계산합니다.
IsBuyDeal(): 거래 여부를 확인합니다.
IsSellDeal(): 매도 거래인지 확인합니다.
GetSpread(): 해당 지점의 가중치를 계산합니다.
IsFullyFilled(): 완전히 동일한지 여부를 확인합니다.
GetFillRate(): 인사를 노력합니다.
ToString(): 제공 정보를 문자열로 반환합니다.

이 모델을 사용하여 최근 딜링 데이터를 관리하고, 딜링 분석 및 성과 평가를 할 수 있습니다. 예를 들어, 거래량 분석, 가격 동향 파악, 분석 분석 등의 기능을 저장할 때 이 모델을 사용할 수 있습니다.
추가 재산이나 방법이 필요하다면 말씀해 주십시오. 
*/