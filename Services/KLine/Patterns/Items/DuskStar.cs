namespace Stock_Online.Services.KLine.Patterns.Items
{
    public class DuskStar : ICandlePattern
    {
        public bool IsMatch(CandleContext ctx)
        {
            var k1 = ctx.At(0);
            var k2 = ctx.At(1);
            var k3 = ctx.At(2);
            if (new[] { k2, k3 }.Any(k => k == null)) return false;

            return
                k1.BodyPct < -3 && k1.LowerShadowPct < 2 &&
                k2.BodyPct < 2 && k2.LowerShadowPct < 2 && k2.UpperShadowPct < 2 && k2.Low > k1.High && k2.Low > k3.High &&
                k3.BodyPct > 3 && k3.LowerShadowPct < 2 && k3.UpperShadowPct < 2 && k3.Close < ((k1.Close + k1.Open) / 2)
                ;
        }
    }
}
