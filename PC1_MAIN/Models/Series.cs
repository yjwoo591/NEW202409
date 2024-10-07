using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForexAITradingPC1Main.Models
{
    public class Series
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SeriesTypeCode { get; set; }

        [Required]
        [StringLength(4)]
        public string YearCode { get; set; }

        [Required]
        [StringLength(2)]
        public string MonthCode { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal InitialMarginRate { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal MaintenanceMarginRate { get; set; }

        // Navigation properties
        public virtual ICollection<HogaData> HogaData { get; set; }
        public virtual ICollection<DealData> DealData { get; set; }
        public virtual ICollection<OrderData> OrderData { get; set; }

        public Series()
        {
            IsActive = true;
            HogaData = new HashSet<HogaData>();
            DealData = new HashSet<DealData>();
            OrderData = new HashSet<OrderData>();
        }

        public override string ToString()
        {
            return $"Series: {SeriesTypeCode} {YearCode}{MonthCode}";
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow > ExpiryDate;
        }

        public TimeSpan TimeUntilExpiry()
        {
            return ExpiryDate - DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void UpdateMarginRates(decimal initialMarginRate, decimal maintenanceMarginRate)
        {
            InitialMarginRate = initialMarginRate;
            MaintenanceMarginRate = maintenanceMarginRate;
        }

        public decimal CalculateInitialMargin(decimal contractValue)
        {
            return contractValue * InitialMarginRate;
        }

        public decimal CalculateMaintenanceMargin(decimal contractValue)
        {
            return contractValue * MaintenanceMarginRate;
        }
    }
}


/*
 * 
 * 이 Series.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

데이터베이스의 시리즈 테이블과 매핑되는 속성을 정의합니다.
숫자는 애트리뷰트를 사용합니다.
HogaData 및 DealData 모델과의 관계를 탐색하는 프로퍼티를 포함합니다.
생성을 설정합니다.
시리즈 관리에 필요한 유용한 메서드를 포함합니다.

주요 속성:

Id: 시리즈의 독립입니다.
SeriesTypeCode: 시리즈 유형 코드입니다.
YearCode: 연도 코드입니다.
MonthCode: 월 코드입니다.
만료일: 만기일입니다.
IsActive: 활성 상태 여부입니다.
초기마진율(InitialMarginRate): 초기 증거금율입니다.
MaintenanceMarginRate: 유지 증거 금율입니다.

주요 방법:

ToString(): 시리즈 정보를 문자열로 반환합니다.
IsExpired(): 시리즈가 만기되었으므로 확인합니다.
TimeUntilExpiry(): 만기까지 시간을 소모합니다.
Deactivate(): 시리즈를 소개합니다.
UpdateMarginRates(): 증거금율을 업데이트합니다.
CalculateInitialMargin(): 초기 증거금을 계산합니다.
CalculateMaintenanceMargin(): 유지 증거금을 절약합니다.

이 모델을 사용하여 거래 시리즈 정보를 관리하고, 시스템 거래에서 시리즈 관련 기능을 공유할 수 있습니다. 예를 들어, 활성 시리즈 조회, 만기 시리즈 처리, 증거금 절감 등의 기능을 감당할 때 이 모델을 사용할 수 있습니다.
추가 재산이나 방법이 필요하다면 말씀해 주십시오.


*/