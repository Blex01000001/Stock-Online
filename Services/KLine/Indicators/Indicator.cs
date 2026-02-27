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
        /// 計算相對強弱指標 (Relative Strength Index, RSI)
        /// 公式：100 - (100 / (1 + RS)), 其中 RS = (n日內平均漲幅 / n日內平均跌幅)
        /// </summary>
        public static List<double?> CalculateRsi(IReadOnlyList<StockDailyPrice> prices, int period = 14)
        {
            // 將收盤價轉換為 double 以進行高效能運算
            var closes = prices.Select(x => (double)x.ClosePrice).ToList();
            var result = new List<double?>();

            if (closes.Count <= period) return result;

            // 計算漲跌幅
            var changes = new List<double>();
            for (int i = 1; i < closes.Count; i++)
            {
                changes.Add(closes[i] - closes[i - 1]);
            }

            for (int i = 0; i < closes.Count; i++)
            {
                // RSI 需要足夠的樣本天數 (至少要有 period 天的漲跌資料)
                if (i < period)
                {
                    result.Add(null);
                    continue;
                }

                // 取過去 period 天的漲跌資料
                var periodChanges = changes.Skip(i - period).Take(period).ToList();

                double avgGain = periodChanges.Where(c => c > 0).DefaultIfEmpty(0).Sum() / (double)period;
                double avgLoss = Math.Abs(periodChanges.Where(c => c < 0).DefaultIfEmpty(0).Sum()) / (double)period;

                if (avgLoss <= 0)
                {
                    result.Add(100.0); // 如果都沒有跌，RSI 為 100
                    continue;
                }

                double rs = avgGain / avgLoss;
                double rsi = 100.0 - (100.0 / (1.0 + rs));

                // 保留兩位小數，符合圖表顯示習慣
                result.Add(Math.Round(rsi, 2));

            }
            return result;
        }

        /// <summary>
        /// 計算標準差 (Standard Deviation)
        /// 用於布林通道：中軌 (SMA) +/- (2 * 標準差)
        /// </summary>
        public static List<decimal?> CalculateStandardDeviation(IReadOnlyList<StockDailyPrice> prices, int period)
        {
            var closes = prices.Select(x => x.ClosePrice).ToList();
            var result = new List<decimal?>();

            for (int i = 0; i < closes.Count; i++)
            {
                if (i + 1 < period)
                {
                    result.Add(null);
                    continue;
                }

                // 1. 取得當前週期的收盤價片段
                var segment = closes.Skip(i - period + 1).Take(period).ToList();

                // 2. 計算平均值 (Mean)
                decimal average = segment.Average();

                // 3. 計算平方差總和 (Sum of Squares)
                double sumOfSquares = segment.Select(val => Math.Pow((double)(val - average), 2)).Sum();

                // 4. 計算標準差 (變異數的平方根)
                decimal stdDev = (decimal)Math.Sqrt(sumOfSquares / period);

                result.Add(Math.Round(stdDev, 2));
            }
            return result;
        }

        /// <summary>
        /// 計算布林通道 (Bollinger Bands)
        /// </summary>
        /// <param name="prices">原始股價資料</param>
        /// <param name="period">週期，預設為 20</param>
        /// <param name="multiplier">倍數，預設為 2</param>
        public static BollingerBandsDto CalculateBollingerBands(IReadOnlyList<StockDailyPrice> prices, int period = 20, int multiplier = 2)
        {
            // 1. 中軌就是簡單移動平均線 (SMA)
            var mid = CalculateSma(prices, period);

            // 2. 計算標準差
            var stdDev = CalculateStandardDeviation(prices, period);

            var result = new BollingerBandsDto { Mid = mid };

            for (int i = 0; i < prices.Count; i++)
            {
                if (mid[i] == null || stdDev[i] == null)
                {
                    result.Upper.Add(null);
                    result.Lower.Add(null);
                    continue;
                }

                // 3. 上軌 = 中軌 + (倍數 * 標準差)
                decimal upper = mid[i].Value + (stdDev[i].Value * multiplier);
                // 4. 下軌 = 中軌 - (倍數 * 標準差)
                decimal lower = mid[i].Value - (stdDev[i].Value * multiplier);

                result.Upper.Add(Math.Round(upper, 2));
                result.Lower.Add(Math.Round(lower, 2));
            }

            return result;
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
