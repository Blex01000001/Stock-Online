using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Enums;

namespace Stock_Online.Services.KLine.Patterns.Items
{
    public class TweezerBottom : ICandlePattern
    {
        public bool IsMatch(CandleContext ctx)
        {
            var k_5 = ctx.At(-5);
            var k0 = ctx.At(0);
            var k1 = ctx.At(1);
            var k2 = ctx.At(2);
            var k10 = ctx.At(10);

            if (new[] { k_5, k0, k1, k2, k10 }.Any(k => k == null))
                return false;

            return
                k0.BodyPct < -3 && k0.LowerShadowPct < 1 &&
                k1.BodyPct > 3 && k1.LowerShadowPct < 1 &&
                k1.Open < k0.Close * 1.002m &&
                k1.Open > k0.Close * 0.998m &&
                k2.Close > k1.Close &&
                k_5.MA(5) > (k0.Open + k0.Close) / 2;
        }
    }
}
