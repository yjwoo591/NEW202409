using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForexAITradingPC1Main.Models
{
    public class AccountStatusLog
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AvailableBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MarginUsed { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnrealizedPnL { get; set; }

        [Required]
        [StringLength(50)]
        public string PartitionKey { get; set; }
    }
}