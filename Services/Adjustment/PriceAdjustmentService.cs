using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Enums;

namespace Stock_Online.Services.Adjustment
{
    public class PriceAdjustmentService : IPriceAdjustmentService
    {
        public IReadOnlyList<StockDailyPrice> AdjustPrices(IReadOnlyList<StockDailyPrice> prices, IReadOnlyList<StockCorporateAction> actions)
        {
            if (actions.Count == 0) return prices;
            List<StockDailyPrice> newPrices = new List<StockDailyPrice>();
            // 1. 確保價格從「最新」到「最舊」排序 (由近到遠回溯)
            var sortedPrices = prices.OrderByDescending(p => p.TradeDate).ToList();

            // 2. 確保分割事件也從「最新」到「最舊」排序
            var sortedSplits = actions.OrderByDescending(s => s.ExDate).ToList();

            decimal cumulativeFactor = 1.0m;
            int splitIdx = 0;

            Console.WriteLine($"splits.Count {sortedSplits.Count} splitIdx {splitIdx} {sortedSplits[splitIdx].StockId}");

            foreach (var p in sortedPrices)
            {
                // 3. 如果當前的股價日期「早於」分割日，代表受到該次分割影響
                while (splitIdx < sortedSplits.Count &&
                    p.StockId == sortedSplits[splitIdx].StockId &&
                    p.TradeDate <= sortedSplits[splitIdx].ExDate &&
                    sortedSplits[splitIdx].ActionType == CorporateActionType.Split)
                {
                    // 累乘倍數（例如：如果經歷兩次 1 拆 2，這裡會變 2 * 2 = 4）
                    cumulativeFactor *= sortedSplits[splitIdx].Ratio.Value;
                    splitIdx++;
                }

                newPrices.Add(new StockDailyPrice()
                {
                    StockId = p.StockId,
                    TradeDate = p.TradeDate,
                    Volume = (long)(p.Volume * cumulativeFactor),
                    Amount = p.Amount,
                    OpenPrice = p.OpenPrice / cumulativeFactor,
                    HighPrice = p.HighPrice / cumulativeFactor,
                    LowPrice = p.LowPrice / cumulativeFactor,
                    ClosePrice = p.ClosePrice / cumulativeFactor,
                    PriceChange = p.PriceChange / cumulativeFactor,
                    TradeCount = p.TradeCount,
                    Note = p.Note,
                });
            }
            return newPrices;
        }
    }
}
