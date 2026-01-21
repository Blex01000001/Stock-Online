using Stock_Online.Domain.Entities;

namespace Stock_Online.Services.KLine.Indicators
{
    public class MovingAverageCalculator : IMovingAverageCalculator
    {
        private static readonly int[] MA_DAYS = { 5, 20, 60, 120, 240 };
        public Dictionary<int, List<decimal?>> Calculate(List<StockDailyPrice> prices)
        {
            var closes = prices.Select(x => x.ClosePrice).ToList();

            Dictionary<int, List<decimal?>> maMap = MA_DAYS.ToDictionary(
                p => p,
                p => CalcMA(closes, p)
            );

            return maMap;
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
