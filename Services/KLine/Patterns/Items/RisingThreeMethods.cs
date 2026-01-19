namespace Stock_Online.Services.KLine.Patterns.Items
{
    public class RisingThreeMethods : ICandlePattern
    {
        public bool IsMatch(CandleContext ctx)
        {
            var k1 = ctx.At(0);
            var k2 = ctx.At(1);
            var k3 = ctx.At(2);
            var k4 = ctx.At(3);
            var k5 = ctx.At(4);
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
    }
}
