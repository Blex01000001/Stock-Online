using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.DTOs;
using Stock_Online.Services.KLine.Builders;
using Stock_Online.Services.KLine.Indicators;
using Stock_Online.Services.KLine.Queries;
using Stock_Online.Services.KLine.Patterns;
using SqlKata;
using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Enums;

namespace Stock_Online.Services.KLine
{
    public class KLineChartService : IKLineChartService
    {
        private readonly IStockRepository _stockDailyrepo;
        private readonly IMovingAverageCalculator _MAcalculator;

        public KLineChartService(IStockRepository repo, IMovingAverageCalculator mAcalculator)
        {
            this._stockDailyrepo = repo;
            this._MAcalculator = mAcalculator;
        }

        public async Task<List<KLineChartDto>> GetKLineAsync(
            string stockId,
            int? days,
            string? start,
            string? end
        )
        {
            Query query = StockDailyPriceQueryBuilder.Build(stockId, null, start, end);
            List<StockDailyPrice> prices = (await _stockDailyrepo.GetPriceByQueryAsync(query))
                .OrderBy(x => x.TradeDate)
                .ToList();

            Query queryAll = StockDailyPriceQueryBuilder.Build(stockId, null, "19110101", end);
            List<StockDailyPrice> pricesAll = (await _stockDailyrepo.GetPriceByQueryAsync(queryAll))
                .OrderBy(x => x.TradeDate)
                .ToList();

            var allPriceDic = pricesAll.ToDictionary(k => k.TradeDate, v => v);
            int index = pricesAll.IndexOf(allPriceDic[prices[0].TradeDate]);

            Dictionary<int, List<decimal?>> maMap = _MAcalculator.Calculate(pricesAll);

            return new List<KLineChartDto> { new KLineChartBuilder(stockId, pricesAll, index, maMap, false).CreateSingle() };
        }
        public async Task<List<KLineChartDto>> GetKPatternLineAsync(string stockId, CandlePattern candlePattern)
        {
            Console.WriteLine($"Get K Line: {stockId}");
            Query query = StockDailyPriceQueryBuilder.Build(stockId, null, "20200101", "20261231");

            List<StockDailyPrice> prices = (await _stockDailyrepo.GetPriceByQueryAsync(query))
                .OrderBy(x => x.TradeDate)
                .ToList();

            Dictionary<int, List<decimal?>> maMap = _MAcalculator.Calculate(prices);

            List<KLineChartDto> klines = new();

            for (int n = 0; n < prices.Count; n++)
            {
                var detector = new CandlePatternDetector(prices, n, maMap)
                    .IsMatch(candlePattern);
                if (!detector) continue;

                //只在「確定命中」後才切 specPrices（給前端）
                klines.Add(new KLineChartBuilder(stockId,prices,n,maMap).Create());
            }
            return klines;
        }
    }
}
