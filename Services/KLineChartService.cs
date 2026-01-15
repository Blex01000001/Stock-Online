using SqlKata;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using Stock_Online.Services.Interface;

namespace Stock_Online.Services
{
    public class KLineChartService : IKLineChartService
    {
        private readonly IStockDailyPriceRepository _repo;

        private static readonly int[] MA_DAYS = { 5, 20, 60, 120, 240 };

        public KLineChartService(IStockDailyPriceRepository repo)
        {
            _repo = repo;
        }

        public async Task<KLineChartDto> GetKLineAsync(
            string stockId,
            int? days,
            string? start,
            string? end
        )
        {
            Query query = new Query("StockDailyPrice")
                .Where("StockId", stockId);

            if (!string.IsNullOrWhiteSpace(start))
            {
                var startDate = DateTime.ParseExact(start, "yyyyMMdd", null);
                query.Where("TradeDate", ">=", startDate);
            }

            if (!string.IsNullOrWhiteSpace(end))
            {
                var endDate = DateTime.ParseExact(end, "yyyyMMdd", null);
                query.Where("TradeDate", "<=", endDate);
            }

            if (days.HasValue)
            {
                query.OrderByDesc("TradeDate").Limit(days.Value);
            }

            var prices = (await _repo.GetByQueryAsync(query))
                .OrderBy(x => x.TradeDate)
                .ToList();

            var points = prices.Select(x => new KLinePointDto
            {
                Date = x.TradeDate.ToString("yyyy-MM-dd"),
                Value = new[]
                {
                x.OpenPrice,
                x.ClosePrice,
                x.LowPrice,
                x.HighPrice
            },
                Volume = x.Volume
            }).ToList();

            var closes = prices.Select(x => x.ClosePrice).ToList();

            var maLines = MA_DAYS.Select(ma =>
                new MALineDto
                {
                    Name = $"MA{ma}",
                    Values = CalcMA(closes, ma)
                }
            ).ToList();

            return new KLineChartDto
            {
                StockId = stockId,
                Points = points,
                MALines = maLines
            };
        }

        /// <summary>
        /// 計算移動平均，前面不足的補 null（與日期對齊）
        /// </summary>
        private static List<decimal?> CalcMA(List<decimal> values, int period)
        {
            var result = new List<decimal?>();

            for (int i = 0; i < values.Count; i++)
            {
                if (i + 1 < period)
                {
                    result.Add(null);
                    continue;
                }

                decimal sum = 0;
                for (int j = i; j > i - period; j--)
                    sum += values[j];

                result.Add(Math.Round(sum / period, 2));
            }

            return result;
        }
    }
}
