using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.DTOs;
using Stock_Online.Services.KLine.Builders;
using Stock_Online.Services.KLine.Indicators;
using Stock_Online.Services.KLine.Queries;
using Stock_Online.Services.KLine.Patterns;
using Stock_Online.Services.KLine.Patterns.Enum;
using SqlKata;
using Stock_Online.Domain.Entities;

namespace Stock_Online.Services.KLine
{
    public class KLineChartService : IKLineChartService
    {
        private readonly IStockDailyPriceRepository _stockDailyrepo;
        private readonly IMovingAverageCalculator _MAcalculator;

        public KLineChartService(IStockDailyPriceRepository repo, IMovingAverageCalculator mAcalculator)
        {
            this._stockDailyrepo = repo;
            this._MAcalculator = mAcalculator;
        }

        public async Task<KLineChartDto> GetKLineAsync(
            string stockId,
            int? days,
            string? start,
            string? end
        )
        {
            throw new NotImplementedException();
            //Query query = new Query("StockDailyPrice")
            //    .Where("StockId", stockId);

            //if (!string.IsNullOrWhiteSpace(start))
            //{
            //    var startDate = DateTime.ParseExact(start, "yyyyMMdd", null);
            //    query.Where("TradeDate", ">=", startDate);
            //}

            //if (!string.IsNullOrWhiteSpace(end))
            //{
            //    var endDate = DateTime.ParseExact(end, "yyyyMMdd", null);
            //    query.Where("TradeDate", "<=", endDate);
            //}

            //if (days.HasValue)
            //{
            //    query.OrderByDesc("TradeDate").Limit(days.Value);
            //}

            //List<StockDailyPrice> prices = (await _stockDailyrepo.GetByQueryAsync(query))
            //    .OrderBy(x => x.TradeDate)
            //    .ToList();

            //var points = prices.Select(x => new KLinePointDto
            //{
            //    Date = x.TradeDate.ToString("yyyy-MM-dd"),
            //    Value = new[]
            //    {
            //    x.OpenPrice,
            //    x.ClosePrice,
            //    x.LowPrice,
            //    x.HighPrice
            //},
            //    Volume = x.Volume
            //}).ToList();

            //var closes = prices.Select(x => x.ClosePrice).ToList();

            //var maLines = MA_DAYS.Select(ma =>
            //    new MALineDto
            //    {
            //        Name = $"MA{ma}",
            //        Values = CalcMA(closes, ma)
            //    }
            //).ToList();

            //return new KLineChartDto
            //{
            //    StockId = stockId,
            //    Points = points,
            //    MALines = maLines
            //};
        }
        public async Task<List<KLineChartDto>> GetKPatternLineAsync(string stockId, CandlePattern candlePattern)
        {
            Console.WriteLine($"Get K Line: {stockId}");
            Query query = StockDailyPriceQueryBuilder.Build(stockId, null, "20200101", "20261231");

            List<StockDailyPrice> prices = (await _stockDailyrepo.GetByQueryAsync(query))
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
