using Stock_Online.Domain.Entities;

namespace Stock_Online.Services.KLine.Patterns
{
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
}
