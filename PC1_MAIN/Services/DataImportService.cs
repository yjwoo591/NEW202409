using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ForexAITradingPC1Main.Models;
using ForexAITradingPC1Main.Database;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ForexAITradingPC1Main.Services
{
    public class DataImportService
    {
        private readonly ForexDbContext _context;

        public DataImportService(ForexDbContext context)
        {
            _context = context;
        }

        public async Task ImportHogaDataFromCsv(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                string line;
                bool isFirstLine = true;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue; // Skip header
                    }

                    var values = line.Split(',');
                    var hogaData = new HogaData
                    {
                        SeriesId = int.Parse(values[0]),
                        Timestamp = DateTime.Parse(values[1]),
                        TimeH = values[2],
                        Time = values[3],
                        AskPrice1 = decimal.Parse(values[4]),
                        AskPrice2 = decimal.Parse(values[5]),
                        AskPrice3 = decimal.Parse(values[6]),
                        AskPrice4 = decimal.Parse(values[7]),
                        AskPrice5 = decimal.Parse(values[8]),
                        BidPrice1 = decimal.Parse(values[9]),
                        BidPrice2 = decimal.Parse(values[10]),
                        BidPrice3 = decimal.Parse(values[11]),
                        BidPrice4 = decimal.Parse(values[12]),
                        BidPrice5 = decimal.Parse(values[13]),
                        AskQuantity1 = int.Parse(values[14]),
                        AskQuantity2 = int.Parse(values[15]),
                        AskQuantity3 = int.Parse(values[16]),
                        AskQuantity4 = int.Parse(values[17]),
                        AskQuantity5 = int.Parse(values[18]),
                        BidQuantity1 = int.Parse(values[19]),
                        BidQuantity2 = int.Parse(values[20]),
                        BidQuantity3 = int.Parse(values[21]),
                        BidQuantity4 = int.Parse(values[22]),
                        BidQuantity5 = int.Parse(values[23]),
                        ExpectedPrice = decimal.Parse(values[24]),
                        TotalAskQuantity = int.Parse(values[25]),
                        TotalBidQuantity = int.Parse(values[26]),
                        PartitionKey = values[27]
                    };

                    _context.HogaData.Add(hogaData);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task ImportDealDataFromCsv(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                string line;
                bool isFirstLine = true;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue; // Skip header
                    }

                    var values = line.Split(',');
                    var dealData = new DealData
                    {
                        SeriesId = int.Parse(values[0]),
                        Timestamp = DateTime.Parse(values[1]),
                        TimeH = values[2],
                        Time = values[3],
                        Price = decimal.Parse(values[4]),
                        Quantity = int.Parse(values[5]),
                        Confirmation = values[6],
                        AccumulatedQuantity = int.Parse(values[7]),
                        AskPrice = decimal.Parse(values[8]),
                        BidPrice = decimal.Parse(values[9]),
                        UnfilledQuantity = int.Parse(values[10]),
                        Sign = values[11],
                        PartitionKey = values[12]
                    };

                    _context.DealData.Add(dealData);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task ImportSeriesFromCsv(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                string line;
                bool isFirstLine = true;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue; // Skip header
                    }

                    var values = line.Split(',');
                    var series = new Series
                    {
                        SeriesTypeCode = int.Parse(values[0]),
                        YearCode = values[1],
                        MonthCode = values[2],
                        ExpiryDate = DateTime.Parse(values[3]),
                        IsActive = bool.Parse(values[4]),
                        InitialMarginRate = decimal.Parse(values[5]),
                        MaintenanceMarginRate = decimal.Parse(values[6])
                    };

                    _context.Series.Add(series);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<string>> ValidateImportedData()
        {
            List<string> validationErrors = new List<string>();

            // Check for duplicate HogaData
            var duplicateHogaData = await _context.HogaData
                .GroupBy(h => new { h.SeriesId, h.Timestamp })
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToListAsync();

            if (duplicateHogaData.Any())
            {
                validationErrors.Add($"Duplicate HogaData found for {duplicateHogaData.Count} entries.");
            }

            // Check for duplicate DealData
            var duplicateDealData = await _context.DealData
                .GroupBy(d => new { d.SeriesId, d.Timestamp })
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToListAsync();

            if (duplicateDealData.Any())
            {
                validationErrors.Add($"Duplicate DealData found for {duplicateDealData.Count} entries.");
            }

            // Check for HogaData and DealData with non-existent SeriesId
            var existingSeriesIds = await _context.Series.Select(s => s.Id).ToListAsync();
            var invalidHogaDataSeriesIds = await _context.HogaData
                .Where(h => !existingSeriesIds.Contains(h.SeriesId))
                .Select(h => h.SeriesId)
                .Distinct()
                .ToListAsync();

            if (invalidHogaDataSeriesIds.Any())
            {
                validationErrors.Add($"HogaData contains invalid SeriesIds: {string.Join(", ", invalidHogaDataSeriesIds)}");
            }

            var invalidDealDataSeriesIds = await _context.DealData
                .Where(d => !existingSeriesIds.Contains(d.SeriesId))
                .Select(d => d.SeriesId)
                .Distinct()
                .ToListAsync();

            if (invalidDealDataSeriesIds.Any())
            {
                validationErrors.Add($"DealData contains invalid SeriesIds: {string.Join(", ", invalidDealDataSeriesIds)}");
            }

            return validationErrors;
        }
    }
}


/*
 * 이 DataImportService.cs 파일은 다음과 같은 주요 특징을 가지고 있습니다:

CSV 파일에서 호가 데이터(HogaData)를 가져오는 기능이 있습니다.
CSV 파일에서 거래 데이터(DealData)를 가져오는 기능이 있습니다.
CSV 파일에서 시리즈 데이터(Series)를 가져오는 기능
가져온 데이터를 조사하는 기능

주요 방법:

ImportHogaDataFromCsv: CSV 파일에서 호가 데이터를 입력하는 데이터베이스에 저장합니다.
ImportDealDataFromCsv: CSV 파일에서 거래 데이터를 입력한 데이터베이스에 생성합니다.
ImportSeriesFromCsv: CSV 파일에서 시리즈 데이터를 입력하는 데이터베이스에 저장합니다.
ValidateImportedData: 돌아온 데이터를 확인합니다. 추출된 데이터를 추출하여 확인합니다.

이 서비스는 다음과 같은 방식으로 데이터를 가져오고 검증을 수행합니다.

CSV 파일을 포함하여 태그 모델(HogaData, DealData, Series)을 생성합니다.
존재하지 않는 데이터베이스 컨텍스트에 추가 내용을 생성합니다.
모든 데이터를 읽는 것은 데이터베이스에 변경 사항을 저장합니다.
데이터 분석 시 유용한 데이터와 잘못된 SeriesId를 참조하십시오.

이 서비스를 사용하여 외부 데이터 소스 데이터를 거대 시스템에 통합할 수 있습니다. 또한, 가져온 데이터의 끌어오기를 확인하여 데이터 품질을 선택할 수 있습니다.
추가 기능이나 수정이 필요하시면 말씀해 주세요.

*/