using SqlKata;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using Stock_Online.Services.Interface;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;

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

            List<StockDailyPrice> prices = (await _repo.GetByQueryAsync(query))
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
        public async Task<List<KLineChartDto>> GetKMultipleLineAsync(string stockId, int? days, string? start, string? end)
        {
            Console.WriteLine($"GetKMultipleLineAsync {stockId}");
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

            //==========================================================================================
            List<StockDailyPrice> prices = (await _repo.GetByQueryAsync(query))
                .OrderBy(x => x.TradeDate)
                .ToList();

            /// ⭐【1】先算 MA（一次、用原始 prices）
            var closes = prices.Select(x => x.ClosePrice).ToList();

            var maMap = MA_DAYS.ToDictionary(
                p => p,
                p => CalcMA(closes, p)
            );

            List<KLineChartDto> klines = new();

            for (int n = 0; n < prices.Count; n++)
            {
                /// ⭐【2】Pattern + MA 一律用「原始 prices + global index」
                var ctx = new CandleContext(prices, n, maMap);
                var detector = new CandlePatternDetector(prices, n, maMap);

                if (!detector.IsFlatBottom()) continue;

                /// ⭐【3】只在「確定命中」後才切 specPrices（給前端）
                int left = Math.Max(0, n - 50);
                int right = Math.Min(prices.Count - 1, n + 50);

                var specPrices = prices
                    .Skip(left)
                    .Take(right - left + 1)
                    .ToList();

                int baseIndex = n - left;

                klines.Add(CreateKline(
                    stockId,
                    specPrices,
                    baseIndex,
                    maMap,
                    left
                ));
            }

            return klines;


            //List<StockDailyPrice> prices = (await _repo.GetByQueryAsync(query))
            //    .OrderBy(x => x.TradeDate)
            //    .ToList();

            //// ⭐ 1. 先算好 MA（一次）
            //var closes = prices.Select(x => x.ClosePrice).ToList();
            //var maMap = MA_DAYS.ToDictionary(
            //    p => p,
            //    p => CalcMA(closes, p)
            //);

            //// ⭐ 2. 再開始逐根 K 線判斷
            //List<KLineChartDto> klines = new List<KLineChartDto>();
            //for (int n = 0; n < prices.Count; n++)
            //{
            //    var ctx = new CandleContext(prices, n, maMap);
            //    var detector = new CandlePatternDetector(prices, n, maMap);

            //    if (!detector.IsFlatBottom()) continue;

            //    int left = Math.Max(0, n - 240);
            //    int right = Math.Min(prices.Count - 1, n + 50);

            //    var specPrices = prices
            //        .Skip(left)
            //        .Take(right - left + 1)
            //        .ToList();

            //    // 基準日 index（在 specPrices 裡）
            //    int baseIndex = n - left;

            //    if (baseIndex < 0 || baseIndex >= specPrices.Count)
            //        continue; // 理論上不會發生，保險

            //    klines.Add(CreateKline(stockId, specPrices, baseIndex));
            //}
            //return klines;
        }
        private KLineChartDto CreateKline(string stockId, List<StockDailyPrice> prices, int baseIndex, Dictionary<int, List<decimal?>> maMap, int left)
        {
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

            var maLines = MA_DAYS.Select(period =>
            {
                var fullMa = maMap[period];

                return new MALineDto
                {
                    Name = $"MA{period}",
                    Values = fullMa
                        .Skip(left)
                        .Take(prices.Count)
                        .ToList()
                };
            }).ToList();

            var markLines = new List<KLineMarkLineDto>();

            int targetIndex = baseIndex + 20;

            if (targetIndex < prices.Count)
            {
                markLines.Add(new KLineMarkLineDto
                {
                    Date = prices[targetIndex].TradeDate.ToString("yyyy-MM-dd"),
                    Type = "N+20",
                    Label = "N+20"
                });
            }
            return new KLineChartDto
            {
                StockId = stockId,
                Points = points,
                MALines = maLines,
                Markers = new List<KLineMarkerDto> { new KLineMarkerDto
                {
                    Date = prices[baseIndex].TradeDate.ToString("yyyy-MM-dd"),
                    Type = "Selected",
                    Label = "N"
                }},
                MarkLines = markLines
            };

        }

        private KLineChartDto CreateKline_OLD(string stockId, List<StockDailyPrice> prices, int dateIndex)
        {
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

            var markLines = new List<KLineMarkLineDto>();

            int targetIndex = dateIndex + 20;

            if (targetIndex < prices.Count)
            {
                markLines.Add(new KLineMarkLineDto
                {
                    Date = prices[targetIndex].TradeDate.ToString("yyyy-MM-dd"),
                    Type = "N+20",
                    Label = "N+20"
                });
            }



            return new KLineChartDto
            {
                StockId = stockId,
                Points = points,
                MALines = maLines,
                Markers = new List<KLineMarkerDto> { new KLineMarkerDto
                {
                    Date = prices[dateIndex].TradeDate.ToString("yyyy-MM-dd"),
                    Type = "Selected",
                    Label = "N"
                }},
                MarkLines = markLines
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

        public class CandleContext
        {
            public IReadOnlyList<StockDailyPrice> Data { get; }
            public int Index { get; }
            private readonly Dictionary<int, List<decimal?>> _ma;

            public StockDailyPrice Current => Data[Index];

            // ===== 原始數值 =====
            public decimal Open => Current.OpenPrice;
            public decimal Close => Current.ClosePrice;
            public decimal High => Current.HighPrice;
            public decimal Low => Current.LowPrice;

            // ===== K 線結構 =====
            public decimal Body { get; }
            public decimal UpperShadow { get; }
            public decimal LowerShadow { get; }

            // ===== 比例（%） =====
            public decimal BodyPct { get; }
            public decimal UpperShadowPct { get; }
            public decimal LowerShadowPct { get; }

            public CandleContext(IReadOnlyList<StockDailyPrice> data, int index, Dictionary<int, List<decimal?>> ma)
            {
                Data = data ?? throw new ArgumentNullException(nameof(data));
                Index = index;
                _ma = ma;

                if (index < 0 || index >= data.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                var d = Current;

                Body = (d.ClosePrice - d.OpenPrice);

                var bodyTop = Math.Max(d.OpenPrice, d.ClosePrice);
                var bodyBottom = Math.Min(d.OpenPrice, d.ClosePrice);

                UpperShadow = Math.Max(0, d.HighPrice - bodyTop);
                LowerShadow = Math.Max(0, bodyBottom - d.LowPrice);

                if (d.ClosePrice > 0)
                {
                    BodyPct = Math.Round(Body / d.ClosePrice * 100m, 2);
                    UpperShadowPct = Math.Round(UpperShadow / d.ClosePrice * 100m, 2);
                    LowerShadowPct = Math.Round(LowerShadow / d.ClosePrice * 100m, 2);
                }
                else
                {
                    BodyPct = 0;
                    UpperShadowPct = 0;
                    LowerShadowPct = 0;
                }
            }

            // ===== Index 安全操作 =====
            public bool HasPrev(int n = 1) => Index - n >= 0;
            public bool HasNext(int n = 1) => Index + n < Data.Count;

            public StockDailyPrice Prev(int n = 1) =>
                HasPrev(n) ? Data[Index - n] : null;

            public StockDailyPrice Next(int n = 1) =>
                HasNext(n) ? Data[Index + n] : null;

            // ===== MA 存取 =====
            public decimal? MA(int period)
                => _ma.TryGetValue(period, out var list)
                    ? list[Index]
                    : null;
        }

        public class CandlePatternDetector
        {
            private readonly IReadOnlyList<StockDailyPrice> _data;
            private readonly Dictionary<int, List<decimal?>> _ma;
            private readonly int _index;

            public CandlePatternDetector(IReadOnlyList<StockDailyPrice> data, int index, Dictionary<int, List<decimal?>> ma)
            {
                _data = data ?? throw new ArgumentNullException(nameof(data));
                _index = index;
                _ma = ma;

                if (index < 0 || index >= data.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
            }

            // ===== Context 取得器 =====

            private bool Has(int offset)
                => _index + offset >= 0 && _index + offset < _data.Count;

            private CandleContext Ctx(int offset = 0)
                => Has(offset)
                    ? new CandleContext(_data, _index + offset, _ma)
                    : null;

            // ===== 單根 =====

            public bool IsHammer()
            {
                var c = Ctx();
                if (c == null) return false;

                return
                    c.LowerShadowPct >= c.BodyPct * 2 &&
                    c.UpperShadowPct <= c.BodyPct * 0.3m &&
                    c.BodyPct <= 2.0m &&
                    IsDownTrend();
            }

            // ===== 組合：吞噬 =====

            public bool IsBullishEngulfing()
            {
                var prev = Ctx(-1);
                var curr = Ctx();

                if (prev == null || curr == null) return false;

                return
                    prev.Close < prev.Open &&
                    curr.Close > curr.Open &&
                    curr.Open <= prev.Close &&
                    curr.Close >= prev.Open &&
                    IsDownTrend();
            }

            // ===== 趨勢 =====

            public bool IsDownTrend(int lookback = 5)
            {
                var curr = Ctx();
                var past = Ctx(-lookback);

                if (curr == null || past == null) return false;

                return curr.Close < past.Close;
            }

            public bool IsUpTrend3Way()
            {
                var k1 = Ctx(0);
                var k2 = Ctx(1);
                var k3 = Ctx(2);
                var k4 = Ctx(3);
                var k5 = Ctx(4);
                if (new[] { k2, k3, k4, k5 }.Any(k => k == null)) return false;

                return
                    //第一根大陽線 第一根K線與當前的上升趨勢相吻合，股價在盤中強勢拉升，形成了一根大陽線
                    k1.BodyPct < -4 && k1.UpperShadowPct < 2 && k1.LowerShadowPct < 2 &&
                    //中間K線組 這些K線的實體都局限在第一根大陽線的交易區間內，表明空頭力量還不足以扭轉大局。
                    k2.BodyPct > 1 && Math.Max(k2.Close, k2.Open) < Math.Max(k1.Close, k1.Open) && Math.Min(k2.Close, k2.Open) > Math.Min(k1.Close, k1.Open) &&
                    k3.BodyPct > 1 && Math.Max(k3.Close, k3.Open) < Math.Max(k1.Close, k1.Open) && Math.Min(k3.Close, k3.Open) > Math.Min(k1.Close, k1.Open) &&
                    k4.BodyPct > 1 && Math.Max(k4.Close, k4.Open) < Math.Max(k1.Close, k1.Open) && Math.Min(k4.Close, k4.Open) > Math.Min(k1.Close, k1.Open)
                    //最後一根K線還沒寫 寫完前面四根已經快沒資料了
                    ;
            }
            public bool IsThreeCrow()
            {
                var k1 = Ctx(0);
                var k2 = Ctx(1);
                var k3 = Ctx(2);
                if (new[] { k2, k3}.Any(k => k == null)) return false;

                return
                    k1.BodyPct > 1 && k1.LowerShadowPct < 2 &&
                    k2.BodyPct > 1 && k2.LowerShadowPct < 2 && k2.Close > k1.Open && k2.Close < k1.Close &&
                    k3.BodyPct > 1 && k3.LowerShadowPct < 2 && k3.Close > k2.Open && k3.Close < k2.Close 
                    ;
            }
            public bool IsDuskStar()
            {
                var k1 = Ctx(0);
                var k2 = Ctx(1);
                var k3 = Ctx(2);
                if (new[] { k2, k3 }.Any(k => k == null)) return false;

                return
                    k1.BodyPct < -3 && k1.LowerShadowPct < 2 &&
                    k2.BodyPct < 2 && k2.LowerShadowPct < 2 && k2.UpperShadowPct < 2 && k2.Low > k1.High && k2.Low > k3.High &&
                    k3.BodyPct > 3 && k3.LowerShadowPct < 2 && k3.UpperShadowPct < 2 && k3.Close < ((k1.Close + k1.Open)/2)
                    ;
            }
            public bool IsFlatBottom()
            {
                var k1 = Ctx(0);
                var k2 = Ctx(1);
                var k3 = Ctx(2);
                if (new[] { k2, k3 }.Any(k => k == null)) return false;

                return
                    k1.BodyPct > 2 && k1.LowerShadowPct < 2 &&
                    k2.BodyPct < -2 && k2.LowerShadowPct < 2 && k2.Close < k1.Open * 1.005m && k2.Close > k1.Open * 0.995m &&
                    (k3.Open + k3.Close)/2 > k2.Open
                    ;
            }

        }

    }
}
