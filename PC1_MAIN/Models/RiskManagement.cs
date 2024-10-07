using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForexAITradingPC1Main.Models
{
    [Table("RiskManagement")]
    public class RiskManagement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AccountId { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxLossLimit { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyLossLimit { get; set; }

        [Required]
        public int PositionLimit { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LeverageLimit { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentDailyLoss { get; set; }

        public int OpenPositions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentLeverage { get; set; }

        public RiskManagement()
        {
            LastUpdated = DateTime.UtcNow;
            CurrentDailyLoss = 0;
            OpenPositions = 0;
            CurrentLeverage = 1;
        }

        public bool IsMaxLossLimitExceeded(decimal currentLoss)
        {
            return currentLoss > MaxLossLimit;
        }

        public bool IsDailyLossLimitExceeded()
        {
            return CurrentDailyLoss > DailyLossLimit;
        }

        public bool IsPositionLimitExceeded()
        {
            return OpenPositions >= PositionLimit;
        }

        public bool IsLeverageLimitExceeded()
        {
            return CurrentLeverage > LeverageLimit;
        }

        public void UpdateDailyLoss(decimal amount)
        {
            CurrentDailyLoss += amount;
            LastUpdated = DateTime.UtcNow;
        }

        public void ResetDailyLoss()
        {
            CurrentDailyLoss = 0;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateOpenPositions(int count)
        {
            OpenPositions = count;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateLeverage(decimal leverage)
        {
            CurrentLeverage = leverage;
            LastUpdated = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"Risk Management for Account: {AccountId}";
        }
    }
}


/*
 *  RiskManagement.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

데이터베이스의 RiskManagement 테이블과 매핑되는 속성을 정의합니다.
숫자는 애트리뷰트를 사용합니다.
계정 모델과의 관계를 이용하여 프로퍼티를 포함합니다.
생성을 설정합니다.
외부 관리에 필요한 유용한 메서드를 포함합니다.

주요 속성:

Id: 체육관리 설정의 시작입니다.
AccountId: 아예 없습니다.
MaxLossLimit: 최대 손실 한도입니다.
DailyLossLimit: 하루 손실 한도입니다.
PositionLimit: 선생님 한도입니다.
LeverageLimit: 레버리지한도입니다.
마지막 업데이트: 업데이트 시간입니다.
CurrentDailyLoss: 현재 하루 손실액입니다.
OpenPositions: 현재 독립 기념일입니다.
CurrentLeverage: 현재 거래중 레버리지입니다.

주요 방법:

IsMaxLossLimitExceeded(): 최대 손실 한도 초과 여부를 확인합니다.
IsDailyLossLimitExceeded(): 하루 분량 한도 초과 여부를 확인합니다.
IsPositionLimitExceeded(): 귀하는 한도 초과 여부를 확인합니다.
IsLeverageLimitExceeded(): 레버리지 한도 초과 여부를 확인합니다.
UpdateDailyLoss(): 매일 손실을 업데이트합니다.
ResetDailyLoss(): 하루에 적은 액수를 가져옵니다.
UpdateOpenPositions(): 열린 북극곰을 업데이트합니다.
UpdateLeverage(): 현재 레버리지를 업데이트합니다.

이 모델을 사용하여 프로토콜을 관리하고 정보를 관리하고, 서비스 시스템에서 서비스 제어를 관리할 수 있습니다. 예를 들어, 거래 실행 전에는 제외 한도를 확인하거나, 매일 손실된 액을 추적하는 등의 기능을 유지하면서 이 모델을 사용할 수 있습니다.
추가 재산이나 방법이 필요하다면 말씀해 주십시오.

*/