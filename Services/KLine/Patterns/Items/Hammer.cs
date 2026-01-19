namespace Stock_Online.Services.KLine.Patterns.Items
{
    public class Hammer : ICandlePattern
    {
        public bool IsMatch(CandleContext ctx)
        {
            var c = ctx.At(0);
            if (c == null) return false;

            return
                c.LowerShadowPct >= c.BodyPct * 2 &&
                c.UpperShadowPct <= c.BodyPct * 0.3m &&
                c.BodyPct <= 2.0m
                ;
        }
    }
}
