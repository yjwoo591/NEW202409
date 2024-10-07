using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForexAITradingPC1Main.Models
{
    [Table("HogaData")]
    public class HogaData
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

        [Column(TypeName = "decimal(18,6)")]
        public decimal AskPrice1 { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal AskPrice2 { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal AskPrice3 { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal AskPrice4 { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal AskPrice5 { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal BidPrice1 { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal BidPrice2 { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal BidPrice3 { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal BidPrice4 { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal BidPrice5 { get; set; }

        public int AskQuantity1 { get; set; }
        public int AskQuantity2 { get; set; }
        public int AskQuantity3 { get; set; }
        public int AskQuantity4 { get; set; }
        public int AskQuantity5 { get; set; }

        public int BidQuantity1 { get; set; }
        public int BidQuantity2 { get; set; }
        public int BidQuantity3 { get; set; }
        public int BidQuantity4 { get; set; }
        public int BidQuantity5 { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal ExpectedPrice { get; set; }

        public int TotalAskQuantity { get; set; }
        public int TotalBidQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public string PartitionKey { get; set; }

        public HogaData()
        {
            Timestamp = DateTime.UtcNow;
            TimeH = Timestamp.ToString("HHmmssff");
            Time = Timestamp.ToString("HHmmss");
            PartitionKey = Timestamp.ToString("yyyyMMdd");
        }

        public decimal GetSpread()
        {
            return AskPrice1 - BidPrice1;
        }

        public decimal GetMidPrice()
        {
            return (AskPrice1 + BidPrice1) / 2;
        }

        public int GetTotalVolume()
        {
            return TotalAskQuantity + TotalBidQuantity;
        }

        public bool IsBidDominant()
        {
            return TotalBidQuantity > TotalAskQuantity;
        }

        public decimal GetVolumeImbalance()
        {
            int totalVolume = GetTotalVolume();
            if (totalVolume == 0) return 0;
            return (decimal)(TotalBidQuantity - TotalAskQuantity) / totalVolume;
        }

        public override string ToString()
        {
            return $"HogaData: {Series.ToString()} at {Timestamp} - Ask: {AskPrice1}, Bid: {BidPrice1}";
        }
    }
}


/*
 * 이 HogaData.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

데이터베이스의 HogaData 테이블과 매핑되는 속성을 정의합니다.
숫자는 애트리뷰트를 사용합니다.
시리즈 모델과의 관계를 활용하여 프로퍼티를 포함합니다.
생성을 설정합니다.
호가 데이터 분석에 필수적인 유용한 메서드를 포함합니다.

주요 속성:

Id: 호가 데이터의 구별입니다.
SeriesId: 닌자 시리즈의 예외입니다.
타임스탬프: 호가 데이터의 타임스탬프입니다.
TimeH, 시간: 시간 정보입니다.
AskPrice1-5, BidPrice1-5: 매도/매수호가 가격입니다.
AskQuantity1-5, BidQuantity1-5: 매도/매수호가 수량입니다.
예상 가격: 기대됩니다.
TotalAskQuantity, TotalBidQuantity: 총 매도/매수 수량입니다.
PartitionKey: 관측 키입니다 (일자별 파티셔닝을 위해 사용).

주요 방법:

GetSpread(): 열심히 노력합니다.
GetMidPrice(): 합리적인 가격을 계산합니다.
GetTotalVolume(): 총 거래량을 계산합니다.
IsBidDominant(): 우월한지를 확인합니다.
GetVolumeImbalance(): 믿음/매도 물량을 충전합니다.
ToString(): 호가 데이터를 문자열로 반환합니다.

이 모델을 사용하여 향후 호가 데이터를 관리하고, 시장 분석 및 거래에 대처할 수 있습니다. 예를 들어, 분석하고, 시장 규모 분석을 하고, 주문하면 예측과 같은 기능을 사용할 때 이 모델을 사용할 수 있습니다.
추가 재산이나 방법이 필요하다면 말씀해 주십시오.

*/