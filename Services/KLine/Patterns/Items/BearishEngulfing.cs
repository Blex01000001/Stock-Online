namespace Stock_Online.Services.KLine.Patterns.Items
{
    public class BearishEngulfing : ICandlePattern
    {
        public bool IsMatch(CandleContext ctx)
        {
            var k1 = ctx.At(0);
            var k2 = ctx.At(1);
            var k3 = ctx.At(2);
            if (new[] { k2, k3 }.Any(k => k == null)) return false;

            return
                k1.BodyPct > 4 && k1.LowerShadowPct < 5 &&
                k2.BodyPct < -2 && k2.LowerShadowPct < 5 && k2.UpperShadowPct < 5 && k2.Open > k1.Close && k2.Close < k1.Open
                ;
        }
    }
}
