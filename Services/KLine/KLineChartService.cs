using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.DTOs;
using Stock_Online.Services.KLine.Builders;
using Stock_Online.Services.KLine.Indicators;
using Stock_Online.Services.KLine.Queries;
using Stock_Online.Services.KLine.Patterns;
using SqlKata;
using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Enums;
using System.Collections.Generic;

namespace Stock_Online.Services.KLine
{
    public class KLineChartService : IKLineChartService
    {
        private readonly IStockRepository _repo;

        public KLineChartService(IStockRepository repo)
        {
            this._repo = repo;
        }

        public async Task<List<KLineChartDto>> GetKLineAsync(
            string stockId,
            int? days,
            string? start,
            string? end
        )
        {
            Query query = StockDailyPriceQueryBuilder.Build(stockId, null, start, end);
            List<StockDailyPrice> prices = (await _repo.GetPricesAsync(query))
                .OrderBy(x => x.TradeDate)
                .ToList();

            Query queryAll = StockDailyPriceQueryBuilder.Build(stockId, null, "19110101", end);
            List<StockDailyPrice> pricesAll = (await _repo.GetPricesAsync(queryAll))
                .OrderBy(x => x.TradeDate)
                .ToList();

            var allPriceDic = pricesAll.ToDictionary(k => k.TradeDate, v => v);
            int index = pricesAll.IndexOf(allPriceDic[prices[0].TradeDate]);

            Query ShareholdingQuery = new Query("StockShareholding").Where("StockId", stockId);
            List<StockShareholding> StockShareholding = (await _repo.GetShareHoldingsAsync(ShareholdingQuery))
                .OrderBy(x => x.Date)
                .ToList();

            return new List<KLineChartDto> { new KLineChartBuilder(stockId, pricesAll, index, false).SetHolding(StockShareholding).CreateSingle() };
        }
        public async Task<List<KLineChartDto>> GetKPatternLineAsync(string stockId, CandlePattern candlePattern)
        {
            Console.WriteLine($"Get K Line: {stockId}");
            Query query = StockDailyPriceQueryBuilder.Build(stockId, null, "20200101", "20261231");

            List<StockDailyPrice> prices = (await _repo.GetPricesAsync(query))
                .OrderBy(x => x.TradeDate)
                .ToList();

            Dictionary<int, List<decimal?>> maMap = MovingAverageCalculator.Calculate(prices);

            List<KLineChartDto> klines = new();

            for (int n = 0; n < prices.Count; n++)
            {
                var detector = new CandlePatternDetector(prices, n, maMap)
                    .IsMatch(candlePattern);
                if (!detector) continue;

                //只在「確定命中」後才切 specPrices（給前端）
                klines.Add(new KLineChartBuilder(stockId,prices,n).Create());
            }
            return klines;
        }
    }
}
