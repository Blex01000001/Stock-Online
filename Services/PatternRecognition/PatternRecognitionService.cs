using SqlKata;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.Services.Adjustment;
using Stock_Online.Services.KLine.Queries;
using Stock_Online.Services.PatternRecognition.Models.DTOs;
using Stock_Online.Services.PatternRecognition.Models.Response;
using System.Collections.Concurrent;
using Stock_Online.Services.KLine;
using Microsoft.AspNetCore.Http;

namespace Stock_Online.Services.PatternRecognition
{
    public class PatternRecognitionService : IPatternRecognitionService
    {
        private readonly IStockRepository _repo;
        private readonly IPriceAdjustmentService _priceAdjService;
        private readonly IEnumerable<IKLinePattern> _patterns; // 注入所有已實作的型態
        private readonly IKLineChartService _kLineChartService;

        public PatternRecognitionService(
            IStockRepository repo,
            IPriceAdjustmentService priceAdjService,
            IEnumerable<IKLinePattern> patterns,
            IKLineChartService kLineChartService)
        {
            this._repo = repo;
            this._priceAdjService = priceAdjService;
            this._patterns = patterns;
            this._kLineChartService = kLineChartService;
        }

        // 情境 1 & 2：單檔股票詳細分析
        public async Task<PatternAnalysisResponse> GetPatternAnalysisAsync(string stockId, string? start, string? end)
        {
            // 1. 抓取資料與還原價格 (參考 KLineChartService 邏輯)
            var prices = await GetAdjustedPricesAsync(stockId, start, end);
            var results = new List<PatternMatchResult>();
            foreach (var pattern in _patterns)
            {
                results.AddRange(pattern.Match(prices));
            }

            var kLineCharts = await _kLineChartService.GetKLineAsync(stockId, null, start, end);

            return new PatternAnalysisResponse
            {
                // 這裡可以呼叫原本的 KLineChartService 取得 ChartData
                ChartData = kLineCharts[0],
                DetectedPatterns = results
            };
        }

        // 情境 3：1000 檔股票大範圍掃描
        public async Task<List<StockPatternSummary>> ScanMarketPatternsAsync(List<string> stockIds, string patternName, string? start, string? end)
        {
            stockIds = (await _repo.GetStockInfosAsync())
                .Select(x => x.StockId).Take(300).ToList();


            var summaryList = new ConcurrentBag<StockPatternSummary>();
            var targetPattern = _patterns.FirstOrDefault(p => p.Name == patternName);

            if (targetPattern == null) return new List<StockPatternSummary>();

            // 使用 Parallel 加速處理 1000 檔股票
            await Task.Run(() =>
            {
                Parallel.ForEach(stockIds, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, stockId =>
                {
                    // 注意：在實務上，這裡建議批次抓取資料庫以優化 I/O
                    var prices = GetAdjustedPricesAsync(stockId, start, end).GetAwaiter().GetResult();
                    var matches = targetPattern.Match(prices).ToList();

                    if (matches.Any())
                    {
                        summaryList.Add(new StockPatternSummary { StockId = stockId, Matches = matches });
                    }
                });
            });

            return summaryList.ToList();
        }
        private async Task<List<StockDailyPrice>> GetAdjustedPricesAsync(string stockId, string? start, string? end)
        {
            DateTime startDate = DateTime.ParseExact(start, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

            string effectiveStart = startDate.AddDays(-365).ToString("yyyyMMdd");

            var actionQuery = new Query("StockCorporateAction")
                .Select("StockId", "ActionType", "ExDate", "Ratio", "CashAmount", "Description")
                .Where("StockId", stockId)
                .OrderByDesc("ExDate");
            List<StockCorporateAction> actions = await _repo.GetCorporateActionsAsync(actionQuery);

            Query priceQueryRequired = StockDailyPriceQueryBuilder.Build(stockId, null, effectiveStart, end);
            var pricesRequired = (await _repo.GetPricesAsync(priceQueryRequired))
                .OrderBy(x => x.TradeDate)
                .ToList();
            var adjPricesAll = _priceAdjService.AdjustPrices(pricesRequired, actions)
                    .Where(x => x.TradeDate > startDate)
                    .ToList();
            return adjPricesAll;
        }
    }
}
