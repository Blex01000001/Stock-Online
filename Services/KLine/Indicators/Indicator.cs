using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;

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
        /// 公式：EMA = (今日收盤價 - 昨日EMA) * (2 / (週期+1)) + 昨日EMA
        /// </summary>
        public static List<decimal?> CalculateEma(IReadOnlyList<StockDailyPrice> prices, int period)
        {
            var closes = prices.Select(x => x.ClosePrice).ToList();
            var result = new List<decimal?>();

            decimal multiplier = 2.0m / (period + 1);
            decimal? previousEma = null;

            for (int i = 0; i < closes.Count; i++)
            {
                // 資料不足週期，回傳 null
                if (i + 1 < period)
                {
                    result.Add(null);
                    continue;
                }

                // 當剛好達到週期時，第一個 EMA 通常使用該期間的 SMA 作為初始值
                if (i + 1 == period)
                {
                    decimal sma = closes.Skip(0).Take(period).Average();
                    previousEma = sma;
                    result.Add(Math.Round(sma, 2));
                    continue;
                }

                // 之後使用 EMA 遞迴公式
                decimal currentEma = (closes[i] - previousEma.Value) * multiplier + previousEma.Value;
                result.Add(Math.Round(currentEma, 2));
                previousEma = currentEma;
            }
            return result;
        }

        /// <summary>
        /// 計算加權移動平均線 (Weighted Moving Average, WMA)
        /// 特點：越近的日期權重越高
        /// </summary>
        public static List<decimal?> CalculateWma(IReadOnlyList<StockDailyPrice> prices, int period)
        {
            var closes = prices.Select(x => x.ClosePrice).ToList();
            var result = new List<decimal?>();

            // 權重分母：1 + 2 + ... + period = n(n+1)/2
            int weightDenominator = period * (period + 1) / 2;

            for (int i = 0; i < closes.Count; i++)
            {
                if (i + 1 < period)
                {
                    result.Add(null);
                    continue;
                }

                decimal weightedSum = 0;
                // 從當前索引往回算，權重從 period 遞減到 1
                for (int j = 0; j < period; j++)
                {
                    int weight = period - j;
                    weightedSum += closes[i - j] * weight;
                }

                result.Add(Math.Round(weightedSum / weightDenominator, 2));
            }
            return result;
        }

        /// <summary>
        /// 計算累計移動平均線 (Cumulative Moving Average, CMA)
        /// 公式：(當前收盤價 + 之前所有收盤價總和) / 當前總天數
        /// </summary>
        public static List<decimal?> CalculateCma(IReadOnlyList<StockDailyPrice> prices)
        {
            var closes = prices.Select(x => x.ClosePrice).ToList();
            var result = new List<decimal?>();

            decimal runningSum = 0;

            for (int i = 0; i < closes.Count; i++)
            {
                runningSum += closes[i];
                decimal cma = runningSum / (i + 1);
                result.Add(Math.Round(cma, 2));
            }
            return result;
        }
        /// <summary>
        /// 計算 MACD 指標 (預設 12, 26, 9)
        /// </summary>
        public static MacdDto CalculateMacd(IReadOnlyList<StockDailyPrice> prices, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            var macd = new MacdDto();

            // 1. 計算快線 EMA 與 慢線 EMA
            var fastEma = CalculateEma(prices, fastPeriod);
            var slowEma = CalculateEma(prices, slowPeriod);

            // 2. 計算 DIF (Fast - Slow)
            var dif = new List<decimal?>();
            for (int i = 0; i < prices.Count; i++)
            {
                if (fastEma[i] == null || slowEma[i] == null)
                {
                    dif.Add(null);
                }
                else
                {
                    dif.Add(Math.Round(fastEma[i].Value - slowEma[i].Value, 3));
                }
            }
            macd.Dif = dif;

            // 3. 計算 DEA (DIF 的 EMA)
            // 注意：這裡需要對 DIF 列表進行 EMA 計算。我們需要重載一個針對 List<decimal?> 的 EMA 方法
            macd.Dea = CalculateEmaForList(dif, signalPeriod);

            // 4. 計算 Hist (DIF - DEA)
            for (int i = 0; i < prices.Count; i++)
            {
                if (macd.Dif[i] == null || macd.Dea[i] == null)
                {
                    macd.Hist.Add(null);
                }
                else
                {
                    // 通常公式為 (DIF - DEA) * 2
                    macd.Hist.Add(Math.Round((macd.Dif[i].Value - macd.Dea[i].Value) * 2, 3));
                }
            }

            return macd;
        }

        /// <summary>
        /// 專門給 DIF 這種已經是數列的資料計算 EMA 用
        /// </summary>
        private static List<decimal?> CalculateEmaForList(List<decimal?> values, int period)
        {
            var result = new List<decimal?>();
            decimal multiplier = 2.0m / (period + 1);
            decimal? previousEma = null;

            // 找出第一個非 null 的索引
            int firstValidIndex = values.FindIndex(v => v != null);

            for (int i = 0; i < values.Count; i++)
            {
                // 在第一個 DIF 出現後，還要累積到 period 天才能算第一個 DEA
                if (i < firstValidIndex + period - 1)
                {
                    result.Add(null);
                    continue;
                }

                if (i == firstValidIndex + period - 1)
                {
                    // 第一個值用 SMA
                    decimal sma = values.Skip(firstValidIndex).Take(period).Average() ?? 0;
                    previousEma = sma;
                    result.Add(Math.Round(sma, 3));
                    continue;
                }

                decimal currentEma = (values[i].Value - previousEma.Value) * multiplier + previousEma.Value;
                result.Add(Math.Round(currentEma, 3));
                previousEma = currentEma;
            }
            return result;
        }
    }
}
