using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Enums;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Stock_Online.Services.KLine.Patterns
{
    public class CandlePatternDetector : ICandlePatternDetector
    {
        private readonly IReadOnlyList<StockDailyPrice> _prices;
        private readonly Dictionary<int, List<decimal?>> _maMap;
        private readonly int _index;
        private CandlePattern _pattern;

        public CandlePatternDetector(IReadOnlyList<StockDailyPrice> prices, int index, Dictionary<int, List<decimal?>> maMap)
        {
            _prices = prices ?? throw new ArgumentNullException(nameof(prices));
            _index = index;
            _maMap = maMap;
            
        }
        public bool IsMatch(CandlePattern pattern)
        {
            _pattern = pattern;
            return pattern switch
            {
                CandlePattern.TweezerBottom => IsTweezerBottom(),
                CandlePattern.RisingThreeMethods => IsRisingThreeMethods(),
                CandlePattern.BearishEngulfing => IsBearishEngulfing(),
                CandlePattern.BullishEngulfing => IsBullishEngulfing(),
                CandlePattern.Hammer => IsHammer(),
                _ => false
            };

        }
        private CandleContext Ctx(int offset = 0)
            => Has(offset)
        ? new CandleContext(_prices, _index + offset, _maMap)
        : null;

        private bool Has(int offset)
            => _index + offset >= 0 && _index + offset < _prices.Count;




        public bool IsHammer()
        {
            var c = Ctx();
            if (c == null) return false;

            return
                c.LowerShadowPct >= c.BodyPct * 2 &&
                c.UpperShadowPct <= c.BodyPct * 0.3m &&
                c.BodyPct <= 2.0m
                ;
            //IsDownTrend();
        }
        public bool IsRisingThreeMethods()
        {
            var k1 = Ctx(0);
            var k2 = Ctx(1);
            var k3 = Ctx(2);
            var k4 = Ctx(3);
            var k5 = Ctx(4);
            if (new[] { k2, k3, k4, k5 }.Any(k => k == null)) return false;

            return
                //第一根大陽線 第一根K線與當前的上升趨勢相吻合，股價在盤中強勢拉升，形成了一根大陽線
                k1.BodyPct > 2 && k1.UpperShadowPct < 2 && k1.LowerShadowPct < 2 &&
                //中間K線組 這些K線的實體都局限在第一根大陽線的交易區間內，表明空頭力量還不足以扭轉大局。
                k2.BodyPct > -2 && k2.BodyPct < 0 && Math.Max(k2.Close, k2.Open) < Math.Max(k1.Close, k1.Open) && Math.Min(k2.Close, k2.Open) > Math.Min(k1.Close, k1.Open) &&
                k3.BodyPct > -2 && k3.BodyPct < 0 && Math.Max(k3.Close, k3.Open) < Math.Max(k1.Close, k1.Open) && Math.Min(k3.Close, k3.Open) > Math.Min(k1.Close, k1.Open) &&
                k4.BodyPct > -2 && k4.BodyPct < 0 && Math.Max(k4.Close, k4.Open) < Math.Max(k1.Close, k1.Open) && Math.Min(k4.Close, k4.Open) > Math.Min(k1.Close, k1.Open) &&
                k5.Close > k1.Close //200多檔只有一筆符合
                
                ;
        }
        public bool IsThreeCrow()
        {
            var k1 = Ctx(0);
            var k2 = Ctx(1);
            var k3 = Ctx(2);
            if (new[] { k2, k3 }.Any(k => k == null)) return false;

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
                k3.BodyPct > 3 && k3.LowerShadowPct < 2 && k3.UpperShadowPct < 2 && k3.Close < ((k1.Close + k1.Open) / 2)
                ;
        }
        public bool IsTweezerBottom()
        {
            var k_5 = Ctx(-5);
            var k0 = Ctx(0);
            var k1 = Ctx(1);
            var k2 = Ctx(2);
            var k10 = Ctx(10);
            if (new[] { k_5, k1, k2 ,k10}.Any(k => k == null)) return false;

            return
                k0.BodyPct < -3 && k0.LowerShadowPct < 1 &&
                k1.BodyPct > 3 && k1.LowerShadowPct < 1 && k1.Open < k0.Close * 1.002m && k1.Open > k0.Close * 0.998m &&
                //(k2.Open + k2.Close) / 2 > k1.Open &&
                k2.Close > k1.Close &&
                k_5.MA(5) > (k0.Open + k0.Close)/2 
                //k10.MA(5) > k1.Close * 1.1m
                ;
        }
        public bool IsBullishEngulfing()
        {
            var k1 = Ctx(0);
            var k2 = Ctx(1);
            var k3 = Ctx(2);
            if (new[] { k2, k3 }.Any(k => k == null)) return false;

            return
                k1.BodyPct < -4 && k1.LowerShadowPct < 5 &&
                k2.BodyPct > 2 && k2.LowerShadowPct < 5 && k2.UpperShadowPct < 5 && k2.Open < k1.Close && k2.Close > k1.Open
                ;
        }
        public bool IsBearishEngulfing()
        {
            var k1 = Ctx(0);
            var k2 = Ctx(1);
            var k3 = Ctx(2);
            if (new[] { k2, k3 }.Any(k => k == null)) return false;

            return
                k1.BodyPct < -3 && k1.LowerShadowPct < 2 &&
                k2.BodyPct < 2 && k2.LowerShadowPct < 2 && k2.UpperShadowPct < 2 && k2.Low > k1.High && k2.Low > k3.High &&
                k3.BodyPct > 3 && k3.LowerShadowPct < 2 && k3.UpperShadowPct < 2 && k3.Close < ((k1.Close + k1.Open) / 2)
                ;
        }


    }
}
