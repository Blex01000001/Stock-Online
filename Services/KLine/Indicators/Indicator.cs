using Stock_Online.Domain.Entities;

namespace Stock_Online.Services.KLine.Indicators
{
    public static class Indicator
    {
        /// <summary>
        /// 計算簡單移動平均線 (Simple Moving Average, SMA)
        /// </summary>
        /// <param name="prices">原始股價資料清單</param>
        /// <param name="period">計算週期</param>
        /// <returns>回傳與輸入長度相同的 decimal? 清單，不足週期處為 null</returns>
        public static List<decimal?> CalculateSma(IReadOnlyList<StockDailyPrice> prices, int period)
        {
            var closes = prices.Select(x => x.ClosePrice).ToList();
            var result = new List<decimal?>();

            for (int i = 0; i < closes.Count; i++)
            {
                // 當前索引加 1 若小於週期，則無法計算，填入 null
                if (i + 1 < period)
                {
                    result.Add(null);
                    continue;
                }

                decimal sum = 0;
                for (int j = i; j > i - period; j--)
                {
                    sum += closes[j];
                }

                result.Add(Math.Round(sum / period, 2));
            }
            return result;
        }

        /// <summary>
        /// 計算指數移動平均線 (Exponential Moving Average, EMA)
        /// TODO: 待實作
        /// </summary>
        public static List<decimal?> CalculateEma(IReadOnlyList<StockDailyPrice> prices, int period)
        {
            // 未來實作邏輯
            throw new NotImplementedException("EMA calculation not implemented yet.");
        }

        /// <summary>
        /// 計算加權移動平均線 (Weighted Moving Average, WMA)
        /// TODO: 待實作
        /// </summary>
        public static List<decimal?> CalculateWma(IReadOnlyList<StockDailyPrice> prices, int period)
        {
            // 未來實作邏輯
            throw new NotImplementedException("WMA calculation not implemented yet.");
        }

        /// <summary>
        /// 計算累計移動平均線 (Cumulative Moving Average, CMA)
        /// TODO: 待實作
        /// </summary>
        public static List<decimal?> CalculateCma(IReadOnlyList<StockDailyPrice> prices)
        {
            // 未來實作邏輯
            throw new NotImplementedException("CMA calculation not implemented yet.");
        }
    }
}
