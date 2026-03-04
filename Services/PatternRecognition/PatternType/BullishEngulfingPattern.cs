using Stock_Online.Domain.Entities;
using Stock_Online.Services.KLine.Patterns;
using Stock_Online.Services.PatternRecognition.Models.DTOs;

namespace Stock_Online.Services.PatternRecognition.PatternType
{
    public class BullishEngulfingPattern : IKLinePattern
    {
        public string Name => "BullishEngulfing";

        public IEnumerable<PatternMatchResult> Match(List<StockDailyPrice> prices)
        {
            for (int i = 1; i < prices.Count; i++)
            {
                CandleContext ctx = new CandleContext(prices, i, null);

                var k_5 = ctx.At(-5);
                var k_2 = ctx.At(-2);
                var k1 = ctx.At(0);
                var k2 = ctx.At(1);
                var k3 = ctx.At(2);
                if (new[] { k_5, k_2,k2, k3 }.Any(k => k == null)) continue;
                if (!(k1.BodyPct < -2 && k1.LowerShadowPct < 1 &&
                k2.BodyPct > 4 && k1.Close > k2.Open && k2.Close > k1.Open &&
                (k_5.Open + k_5.Close) / 2 > (k_2.Open + k_2.Close) / 1.8m &&
                (k_2.Open + k_2.Close) / 2 > (k1.Open + k1.Close) / 2
                )) continue;

                Console.WriteLine($"\tMatch {prices[i].TradeDate} start index: {i} prices count: {prices.Count}");
                Console.WriteLine($"\t 1 open {prices[i].OpenPrice} close{prices[i].ClosePrice} {(prices[i].OpenPrice - prices[i].ClosePrice) / prices[i].ClosePrice}%");
                Console.WriteLine($"\t 2 open {prices[i + 1].OpenPrice} close{prices[i + 1].ClosePrice} {(prices[i + 1].OpenPrice - prices[i+1].ClosePrice) / prices[i+1].ClosePrice}%");

                yield return new PatternMatchResult
                {
                    PatternName = this.Name,
                    StartIndex = i,
                    EndIndex = i + 1,
                    Signal = "Bullish"
                };
            }
        }
        public IEnumerable<PatternMatchResult> Match2(List<StockDailyPrice> prices)
        {
            for (int i = 1; i < prices.Count; i++)
            {
                var prev = prices[i - 1];
                var curr = prices[i];

                // 簡單邏輯：前一根是黑 K，後一根是紅 K 且包覆前一根實體
                if (prev.ClosePrice < prev.OpenPrice &&
                    curr.ClosePrice > curr.OpenPrice &&
                    curr.OpenPrice <= prev.ClosePrice &&
                    curr.ClosePrice >= prev.OpenPrice)
                {
                    yield return new PatternMatchResult
                    {
                        PatternName = this.Name,
                        StartIndex = i - 1,
                        EndIndex = i,
                        Signal = "Bullish"
                    };
                }
            }
        }
    }
}
