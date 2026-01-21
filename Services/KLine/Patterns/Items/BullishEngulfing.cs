namespace Stock_Online.Services.KLine.Patterns.Items
{
    public class BullishEngulfing : ICandlePattern
    {
        public bool IsMatch(CandleContext ctx)
        {
            var k_5 = ctx.At(-5);
            var k1 = ctx.At(0);
            var k2 = ctx.At(1);
            var k3 = ctx.At(2);
            if (new[] { k_5, k2, k3 }.Any(k => k == null)) return false;

            return
                k1.BodyPct < -2 && k1.LowerShadowPct < 5 &&
                k2.BodyPct > 2 && k2.LowerShadowPct < 5 && k2.UpperShadowPct < 5 && k2.Open < k1.Low && k2.Close > k1.High 
                //k1.MA(20) < k1.MA(60) && k1.MA(20) < k1.MA(120) &&
                //k1.MA(20) < k_5.MA(20) &&
                //Math.Abs(k1.Close - k1.MA(60).Value)/k1.MA(60) < 0.1m
                ;
        }
    }
}
