using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Enums;
using System.Diagnostics;

namespace Stock_Online.Services.Adjustment
{
    public class DividendAdjustmentService : IDividendAdjustmentService
    {
        public IReadOnlyList<StockDividend> AdjustDividends(IReadOnlyList<StockDividend> dividends, IReadOnlyList<StockCorporateAction> actions)
        {
            if (actions.Count == 0) return dividends;
            List<StockDividend> newDividends = new List<StockDividend>();
            // 1. 確保價格從「最新」到「最舊」排序 (由近到遠回溯)
            var sortedDividends = dividends.OrderByDescending(p => p.Date).ToList();

            // 2. 確保分割事件也從「最新」到「最舊」排序
            var sortedSplits = actions.OrderByDescending(s => s.ExDate).ToList();

            decimal cumulativeFactor = 1.0m;
            int splitIdx = 0;

            Console.WriteLine($"splits.Count {sortedSplits.Count} splitIdx {splitIdx} {sortedSplits[splitIdx].StockId}");

            foreach (var p in sortedDividends)
            {
                // 3. 如果當前的股價日期「早於」分割日，代表受到該次分割影響
                while (splitIdx < sortedSplits.Count &&
                    p.StockId == sortedSplits[splitIdx].StockId &&
                    p.Date <= sortedSplits[splitIdx].ExDate &&
                    sortedSplits[splitIdx].ActionType == CorporateActionType.Split)
                {
                    // 累乘倍數（例如：如果經歷兩次 1 拆 2，這裡會變 2 * 2 = 4）
                    cumulativeFactor *= sortedSplits[splitIdx].Ratio.Value;
                    splitIdx++;
                }

                newDividends.Add(new StockDividend()
                {
                    StockId = p.StockId,
                    Date = p.Date,
                    Year = p.Year,
                    StockEarningsDistribution = p.StockEarningsDistribution / cumulativeFactor,
                    StockStatutorySurplus = p.StockStatutorySurplus / cumulativeFactor,
                    StockExDividendTradingDate = p.StockExDividendTradingDate,
                    TotalEmployeeStockDividend = p.TotalEmployeeStockDividend / cumulativeFactor,
                    TotalEmployeeStockDividendAmount = p.TotalEmployeeStockDividendAmount / cumulativeFactor,
                    RatioOfEmployeeStockDividendOfTotal = p.RatioOfEmployeeStockDividendOfTotal / cumulativeFactor,
                    RatioOfEmployeeStockDividend = p.RatioOfEmployeeStockDividend / cumulativeFactor,
                    CashEarningsDistribution = p.CashEarningsDistribution / cumulativeFactor,
                    CashStatutorySurplus = p.CashStatutorySurplus / cumulativeFactor,
                    CashExDividendTradingDate = p.CashExDividendTradingDate,
                    CashDividendPaymentDate = p.CashDividendPaymentDate,
                    TotalEmployeeCashDividend = p.TotalEmployeeCashDividend / cumulativeFactor,
                    TotalNumberOfCashCapitalIncrease = p.TotalNumberOfCashCapitalIncrease / cumulativeFactor,
                    CashIncreaseSubscriptionRate = p.CashIncreaseSubscriptionRate / cumulativeFactor,
                    CashIncreaseSubscriptionPrice = p.CashIncreaseSubscriptionPrice / cumulativeFactor,
                    RemunerationOfDirectorsAndSupervisors = p.RemunerationOfDirectorsAndSupervisors / cumulativeFactor,
                    ParticipateDistributionOfTotalShares = p.ParticipateDistributionOfTotalShares / cumulativeFactor,
                    AnnouncementDate = p.AnnouncementDate,
                    AnnouncementTime = p.AnnouncementTime,
                });
            }
            return newDividends;
        }
    }
}
