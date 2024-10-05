using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using ForexAITradingPC1Main.Models;
using ForexAITradingPC1Main.Database;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ClosedXML.Excel;

namespace ForexAITradingPC1Main.Services
{
    public class DataExportService
    {
        private readonly ForexDbContext _context;

        public DataExportService(ForexDbContext context)
        {
            _context = context;
        }

        public async Task ExportHogaDataToCsvAsync(string filePath, DateTime startDate, DateTime endDate)
        {
            var hogaData = await _context.HogaData
                .Where(h => h.Timestamp >= startDate && h.Timestamp <= endDate)
                .OrderBy(h => h.Timestamp)
                .ToListAsync();

            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(hogaData);
            }
        }

        public async Task ExportDealDataToCsvAsync(string filePath, DateTime startDate, DateTime endDate)
        {
            var dealData = await _context.DealData
                .Where(d => d.Timestamp >= startDate && d.Timestamp <= endDate)
                .OrderBy(d => d.Timestamp)
                .ToListAsync();

            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(dealData);
            }
        }

        public async Task ExportAccountSummaryToCsvAsync(string filePath, int accountId, DateTime startDate, DateTime endDate)
        {
            var accountSummaries = await _context.AccountDailySummaries
                .Where(s => s.AccountId == accountId && s.SummaryDate >= startDate && s.SummaryDate <= endDate)
                .OrderBy(s => s.SummaryDate)
                .ToListAsync();

            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(accountSummaries);
            }
        }

        public async Task ExportTradingPerformanceToCsvAsync(string filePath, int accountId, DateTime startDate, DateTime endDate)
        {
            var performance = await _context.TradePerformanceLogs
                .Where(p => p.AccountId == accountId && p.Timestamp >= startDate && p.Timestamp <= endDate)
                .OrderBy(p => p.Timestamp)
                .ToListAsync();

            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(performance);
            }
        }

        public async Task<byte[]> ExportSystemPerformanceToExcelAsync(DateTime startDate, DateTime endDate)
        {
            var performance = await _context.SystemPerformances
                .Where(p => p.Timestamp >= startDate && p.Timestamp <= endDate)
                .OrderBy(p => p.Timestamp)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("System Performance");

                // Add headers
                worksheet.Cell(1, 1).Value = "Timestamp";
                worksheet.Cell(1, 2).Value = "CPU Usage Avg";
                worksheet.Cell(1, 3).Value = "Memory Usage Avg";
                worksheet.Cell(1, 4).Value = "Network Latency Avg";
                worksheet.Cell(1, 5).Value = "Throughput Avg";

                // Add data
                for (int i = 0; i < performance.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = performance[i].Timestamp;
                    worksheet.Cell(i + 2, 2).Value = performance[i].CPUUsageAvg;
                    worksheet.Cell(i + 2, 3).Value = performance[i].MemoryUsageAvg;
                    worksheet.Cell(i + 2, 4).Value = performance[i].NetworkLatencyAvg;
                    worksheet.Cell(i + 2, 5).Value = performance[i].ThroughputAvg;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task ExportAllDataToZipAsync(string zipFilePath, DateTime startDate, DateTime endDate)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                await ExportHogaDataToCsvAsync(Path.Combine(tempDir, "hoga_data.csv"), startDate, endDate);
                await ExportDealDataToCsvAsync(Path.Combine(tempDir, "deal_data.csv"), startDate, endDate);

                var accounts = await _context.Accounts.Select(a => a.Id).ToListAsync();
                foreach (var accountId in accounts)
                {
                    await ExportAccountSummaryToCsvAsync(Path.Combine(tempDir, $"account_summary_{accountId}.csv"), accountId, startDate, endDate);
                    await ExportTradingPerformanceToCsvAsync(Path.Combine(tempDir, $"trading_performance_{accountId}.csv"), accountId, startDate, endDate);
                }

                var excelBytes = await ExportSystemPerformanceToExcelAsync(startDate, endDate);
                File.WriteAllBytes(Path.Combine(tempDir, "system_performance.xlsx"), excelBytes);

                System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, zipFilePath);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
/*
이 DataExportService.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:
이 DataExportService 클래스는 다음과 같은 주요 특징과 메서드를 포함하고 있습니다:

ForexDbContext를 생성하여 수신으로 데이터베이스 작업을 축하합니다.
ImportHogaDataToCsvAsync: 호가 데이터를 CSV 파일로 내보냅니다.
ImportDealDataToCsvAsync: 거래 데이터를 CSV 파일로 내보냅니다.
ImportAccountSummaryToCsvAsync: 마찬가지로 요약 데이터를 CSV 파일로 내보냅니다.
ImportTradingPerformanceToCsvAsync: 거래 성과 데이터를 CSV 파일로 내보냅니다.
ImportSystemPerformanceToExcelAsync: 시스템 성능 데이터를 Excel 파일로 내보냅니다.
ImportAllDataToZipAsync: 모든 데이터를 ZIP 파일로 압축하여 내보냅니다.

모든 메서드는 특정으로 간주되어 I/O 작업이 허용됩니다. 또한, CsvHelper와 ClosedXML 라이브러리를 사용하여 CSV 및 Excel 파일 생성을 처리하고 있습니다.
이 서비스를 사용하기 위해서는 다음 NuGet 패키지가 프로젝트에 설치되어야 합니다.

Microsoft.EntityFrameworkCore
CSV헬퍼
닫힌 XML

다양한 데이터 유형(호가, 거래, 약간 요약, 거래 성능, 시스템 성능)에 대한 기능이 있습니다.
CSV 및 Excel 형식으로 데이터 포함
날짜에 따른 정보
여러 데이터를 ZIP 파일로 압축하여 포함

주요 방법:

ImportHogaDataToCsvAsync: 호가 데이터를 CSV 파일로 내보냅니다.
ImportDealDataToCsvAsync: 거래 데이터를 CSV 파일로 내보냅니다.
ImportAccountSummaryToCsvAsync: 마찬가지로 요약 데이터를 CSV 파일로 내보냅니다.
ImportTradingPerformanceToCsvAsync: 거래 성과 데이터를 CSV 파일로 내보냅니다.
ImportSystemPerformanceToExcelAsync: 시스템 성능 데이터를 Excel 파일로 내보냅니다.
ImportAllDataToZipAsync: 모든 데이터를 ZIP 파일로 내보냅니다.

이 서비스는 다음과 동일한 방식으로 데이터를 수행합니다.

Entity Framework Core를 사용하여 데이터베이스에서 데이터를 쿼리합니다.
CsvHelper 라이브러리를 사용하여 CSV 파일을 생성합니다.
ClosedXML 라이브러리를 사용하여 Excel 파일을 생성합니다.
System.IO.Compression 라벨스페이스를 사용하여 ZIP 파일을 생성합니다.

이 서비스를 사용하여 시스템의 다양한 데이터를 외부 파일로 내부적으로 볼 수 있습니다. 데이터 분석, 백업, 리포팅 등 다양한 목적으로 활용될 수 있습니다.
추가 기능이나 수정이 필요하시면 말씀해 주세요.

*/