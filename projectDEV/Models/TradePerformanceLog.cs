using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForexAITradingPC1Main.Models
{
    public class TradePerformanceLog
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }

        public int DealId { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TradeAmount { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal TradePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RealizedPnL { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionFees { get; set; }

        [Required]
        [StringLength(50)]
        public string PartitionKey { get; set; }
    }
}