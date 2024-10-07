using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForexAITradingPC1Main.Models
{
    public class SystemPerformance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        public int IntervalSeconds { get; set; }

        public float CPUUsageMin { get; set; }
        public float CPUUsageMax { get; set; }
        public float CPUUsageAvg { get; set; }

        public float MemoryUsageMin { get; set; }
        public float MemoryUsageMax { get; set; }
        public float MemoryUsageAvg { get; set; }

        public float NetworkLatencyMin { get; set; }
        public float NetworkLatencyMax { get; set; }
        public float NetworkLatencyAvg { get; set; }

        public int ThroughputMin { get; set; }
        public int ThroughputMax { get; set; }
        public int ThroughputAvg { get; set; }

        public float CPUTemperatureMin { get; set; }
        public float CPUTemperatureMax { get; set; }
        public float CPUTemperatureAvg { get; set; }

        public int FanSpeedMin { get; set; }
        public int FanSpeedMax { get; set; }
        public int FanSpeedAvg { get; set; }

        public float DiskUsageMin { get; set; }
        public float DiskUsageMax { get; set; }
        public float DiskUsageAvg { get; set; }

        public float GPUUsageMin { get; set; }
        public float GPUUsageMax { get; set; }
        public float GPUUsageAvg { get; set; }

        public float GPUTemperatureMin { get; set; }
        public float GPUTemperatureMax { get; set; }
        public float GPUTemperatureAvg { get; set; }

        public float DBQueryResponseTime { get; set; }
        public int DBConnectionCount { get; set; }

        [Required]
        [StringLength(50)]
        public string PartitionKey { get; set; }
    }
}