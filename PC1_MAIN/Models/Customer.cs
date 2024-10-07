using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace ForexAITradingPC1Main.Models
{
    [Table("Customer")]
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(50)]
        public string HtsApiId { get; set; }

        [Required]
        [StringLength(255)]
        public string HtsApiPassword { get; set; }

        [Required]
        [StringLength(255)]
        public string HtsCertPassword { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountType { get; set; }

        [Required]
        [StringLength(20)]
        public string KYCStatus { get; set; }

        [Required]
        public DateTime KYCLastUpdated { get; set; }

        [Required]
        [StringLength(50)]
        public string CustomerCategory { get; set; }

        // Navigation property
        public virtual ICollection<Account> Accounts { get; set; }

        public Customer()
        {
            CreatedAt = DateTime.UtcNow;
            KYCLastUpdated = DateTime.UtcNow;
            Accounts = new HashSet<Account>();
        }

        public override string ToString()
        {
            return $"Customer: {Id} - {Name} ({Email})";
        }

        public bool IsKYCUpToDate()
        {
            return (DateTime.UtcNow - KYCLastUpdated).TotalDays <= 365;
        }

        public void UpdateKYCStatus(string newStatus)
        {
            KYCStatus = newStatus;
            KYCLastUpdated = DateTime.UtcNow;
        }
    }
}


/*
이 Customer.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

데이터베이스의 고객 테이블과 매핑되는 속성을 정의합니다.
숫자는 애트리뷰트를 사용합니다.
계정 모델과의 관계를 이용하여 프로퍼티를 포함합니다.
생성을 설정합니다.
유용한 메서드를 포함합니다(ToString, IsKYCUpToDate, UpdateKYCStatus).

주요 속성:

Id: 귀하의 고유 행위입니다.
이름: 귀하의 이름입니다.
이메일: 귀하의 이메일 주소입니다.
CreatedAt: 고객 계정 생성 대기입니다.
HtsApiId, HtsApiPassword, HtsCertPassword: HTS API 관련 정보입니다.
AccountType: 계정입니다.
KYCStatus: KYC(Know Your Customer) 상태입니다.
KYCastUpdated: 마지막 KYC 업데이트 기간입니다.
CustomerCategory: 귀하를 정의합니다.

주요 방법:

ToString(): 귀하의 정보를 계속해서 반환합니다.
IsKYCUpToDate(): KYC 정보가 최신 상태인지 확인합니다(1년 이내 업데이트).
UpdateKYCStatus(): KYC 상태를 업데이트하고 업데이트 시간을 기록합니다.

이 모델을 사용하여 고객 정보를 관리하고, 데이터베이스 상호 작용 결합, 비즈니스 구조를 방해할 수 있습니다. 예를 들어, 고객 등록, 정보 수정, KYC 상태 확인 등의 기능을 중단할 때 이 모델을 사용할 수 있습니다.
추가 재산이나 방법이 필요하다면 말씀해 주십시오.
*/ 
  