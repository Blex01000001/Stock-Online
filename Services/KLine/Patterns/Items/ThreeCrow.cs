namespace Stock_Online.Services.KLine.Patterns.Items
{
    public class ThreeCrow : ICandlePattern
    {
        public bool IsMatch(CandleContext ctx)
        {
            var k1 = ctx.At(0);
            var k2 = ctx.At(1);
            var k3 = ctx.At(2);
            if (new[] { k2, k3 }.Any(k => k == null)) return false;

            return
                k1.BodyPct > 1 && k1.LowerShadowPct < 2 &&
                k2.BodyPct > 1 && k2.LowerShadowPct < 2 && k2.Close > k1.Open && k2.Close < k1.Close &&
                k3.BodyPct > 1 && k3.LowerShadowPct < 2 && k3.Close > k2.Open && k3.Close < k2.Close
                ;
        }
    }
}
