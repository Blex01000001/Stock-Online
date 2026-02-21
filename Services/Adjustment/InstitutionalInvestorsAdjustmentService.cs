using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Enums;

namespace Stock_Online.Services.Adjustment
{
    public class InstitutionalInvestorsAdjustmentService : IInstitutionalInvestorsAdjustmentService
    {
        /// <summary>
        /// 根據股票分割事件調整外資買賣超數據
        /// </summary>
        /// <param name="buySellData">原始買賣超列表</param>
        /// <param name="actions">公司行動列表（含分割事件）</param>
        /// <returns>調整後的買賣超列表</returns>
        public IReadOnlyList<StockInstitutionalInvestorsBuySell> AdjustBuySell(
            IReadOnlyList<StockInstitutionalInvestorsBuySell> buySellData,
            IReadOnlyList<StockCorporateAction> actions)
        {
            // 如果沒有分割事件，直接回傳原始數據
            if (actions == null || !actions.Any(a => a.ActionType == CorporateActionType.Split))
                return buySellData;

            // 1. 確保買賣超數據從「最新」到「最舊」排序 (由近到遠回溯)
            var sortedBuySell = buySellData.OrderByDescending(b => b.Date).ToList();

            // 2. 確保分割事件也從「最新」到「最舊」排序
            var sortedSplits = actions
                .Where(a => a.ActionType == CorporateActionType.Split)
                .OrderByDescending(s => s.ExDate)
                .ToList();

            List<StockInstitutionalInvestorsBuySell> adjustedList = new List<StockInstitutionalInvestorsBuySell>();

            decimal cumulativeFactor = 1.0m;
            int splitIdx = 0;

            foreach (var record in sortedBuySell)
            {

                // 3. 如果當前的數據日期「早於或等於」分割基準日，代表該歷史數據受到分割影響
                // 需注意：日期格式為 "yyyy-MM-dd"，字串比較在 ISO 格式下是安全的
                while (splitIdx < sortedSplits.Count &&
                       record.StockId == sortedSplits[splitIdx].StockId &&
                       string.Compare(record.Date, sortedSplits[splitIdx].ExDate.ToString("yyyy-MM-dd")) <= 0)
                {
                    // 累乘倍數（例如：1 拆 2，Ratio 為 2.0，歷史買賣股數需乘以 2）
                    cumulativeFactor *= (sortedSplits[splitIdx].Ratio ?? 1.0m);
                    splitIdx++;
                }

                // 4. 建立調整後的物件
                adjustedList.Add(new StockInstitutionalInvestorsBuySell()
                {
                    StockId = record.StockId,
                    Date = record.Date,
                    Name = record.Name,
                    // 進行調整：將歷史買賣數據乘以累計倍數
                    // 使用 Math.Round 處理可能的小數點誤差（通常張數/股數為整數）
                    Buy = (long)Math.Round(record.Buy * cumulativeFactor, MidpointRounding.AwayFromZero),
                    Sell = (long)Math.Round(record.Sell * cumulativeFactor, MidpointRounding.AwayFromZero)
                });
            }

            // 回傳時可以考慮重新按日期升序排序，方便後續顯示
            return adjustedList.OrderBy(x => x.Date).ToList();
        }
    }
}
