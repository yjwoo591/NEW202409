using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForexAITradingPC1Main.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountStatus { get; set; }

        [Required]
        public DateTime LastLoginTimestamp { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AvailableBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MarginUsed { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnrealizedPnL { get; set; }

        // Navigation properties
        public virtual RiskManagement RiskManagement { get; set; }
        public virtual ICollection<AccountStatusLog> AccountStatusLogs { get; set; }
        public virtual ICollection<TradePerformanceLog> TradePerformanceLogs { get; set; }
        public virtual ICollection<AccountDailySummary> AccountDailySummaries { get; set; }
        public virtual ICollection<OrderData> OrderData { get; set; }

        public Account()
        {
            LastLoginTimestamp = DateTime.UtcNow;
            AccountStatus = "Active";
            Balance = 0;
            AvailableBalance = 0;
            AccountStatusLogs = new HashSet<AccountStatusLog>();
            TradePerformanceLogs = new HashSet<TradePerformanceLog>();
            AccountDailySummaries = new HashSet<AccountDailySummary>();
            OrderData = new HashSet<OrderData>();
        }

        public override string ToString()
        {
            return $"Account: {AccountNumber} (Customer: {CustomerId})";
        }

        public bool IsActive()
        {
            return AccountStatus == "Active";
        }

        public void UpdateLastLoginTimestamp()
        {
            LastLoginTimestamp = DateTime.UtcNow;
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive");

            Balance += amount;
            AvailableBalance += amount;
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive");

            if (amount > AvailableBalance)
                throw new InvalidOperationException("Insufficient available balance");

            Balance -= amount;
            AvailableBalance -= amount;
        }

        public void UpdateMarginUsed(decimal marginUsed)
        {
            MarginUsed = marginUsed;
            AvailableBalance = Balance - marginUsed;
        }

        public void UpdateUnrealizedPnL(decimal unrealizedPnL)
        {
            UnrealizedPnL = unrealizedPnL;
        }
    }
}

/*
 * 
 * 
 * 이 Account 클래스는 다음과 같은 주요 특징을 가지고 있습니다:

기본 속성: Id, AccountNumber, Password, CustomerId, AccountStatus, LastLoginTimestamp, Balance, AvailableBalance, MarginUsed, UnrealizedPnL
Navigation 속성:

Customer (일대다 관계의 "다" 쪽)
RiskManagement (일대일 관계)
AccountStatusLogs, TradePerformanceLogs, AccountDailySummaries, OrderData (일대다 관계의 "일" 쪽)


생성자: 기본값 설정 및 컬렉션 초기화
유틸리티 메서드:

ToString(): 계정 정보를 문자열로 반환
IsActive(): 계정 활성 상태 확인
UpdateLastLoginTimestamp(): 마지막 로그인 시간 업데이트
Deposit(): 입금 처리
Withdraw(): 출금 처리
UpdateMarginUsed(): 사용된 마진 업데이트
UpdateUnrealizedPnL(): 미실현 손익 업데이트



이 클래스는 계정과 관련된 모든 데이터와 기능을 캡슐화하고 있으며, Entity Framework Core를 통해 데이터베이스와 매핑됩니다. OrderData와의 관계도 포함되어 있어, 이전에 발생했던 오류도 해결되었을 것입니다.
이 구현으로 Account와 OrderData 사이의 관계 설정 관련 오류가 해결되어야 합니다. 만약 여전히 문제가 발생한다면, ForexDbContext.cs 파일의 관련 부분을 다시 한 번 확인해 주시기 바랍니다.
추가적인 수정이나 기능이 필요하다면 말씀해 주세요.


 * 이 Account.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

데이터베이스의 계정 테이블과 매핑되는 속성을 정의합니다.
숫자는 애트리뷰트를 사용합니다.
고객 및 위험 관리 모델과의 관계를 탐색하는 프로퍼티를 포함합니다.
생성을 설정합니다.
SDK를 관리하는 데 필요한 유용한 메서드를 포함합니다.

주요 속성:

Id: 당연하게도 행위입니다.
AccountNumber: 당연히 번호입니다.
비밀번호: 당연합니다.
CustomerId: 주의사항(귀하)의 행동입니다.
AccountStatus: 당연히 상태입니다.
LastLoginTimestamp: 마지막 로그인 시간입니다.
잔액: 당연합니다.
AvailableBalance: 사용 가능한 잔액입니다.
MarginUsed: 사용된 마진 금액입니다.
UnrealizedPnL: 미실현 손익입니다.

주요 방법:

ToString(): 당연하게도 문자열을 반환합니다.
IsActive(): 당연하게도 유효한 상태인지 확인합니다.
UpdateLastLoginTimestamp(): 마지막 로그인 시간을 업데이트합니다.
Deposit(): 처리해 주세요.
Withdraw(): 출금을 처리합니다.
UpdateMarginUsed(): 사용된 마진을 업데이트하고 가용 잔액을 조정합니다.
UpdateUnrealizedPnL(): 미실현 손익을 업데이트합니다.

이 모델을 사용하여 이상한 정보를 관리하고, 데이터베이스 상호 작용 결합, 비즈니스 구조를 방해할 수 있습니다. 예를 들어, 아파트먼트, 입출금 처리, 마진 거래, 손익 계산 등의 기능을 사용할 때 이 모델을 사용할 수 있습니다.
추가 재산이나 방법이 필요하다면 말씀해 주십시오.

/*/