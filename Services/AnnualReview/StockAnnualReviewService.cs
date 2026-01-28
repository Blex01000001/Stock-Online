using Microsoft.AspNetCore.SignalR;
using SqlKata;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using Stock_Online.Hubs;
using Stock_Online.Services.KLine.Queries;

namespace Stock_Online.Services.AnnualReview
{
    public class StockAnnualReviewService : IStockAnnualReviewService
    {
        private readonly IStockRepository _repo;
        private readonly IHubContext<StockUpdateHub> _hub;

        public StockAnnualReviewService(IStockRepository repo, IHubContext<StockUpdateHub> hub)
        {
            _repo = repo;
            _hub = hub;
        }
        public async Task<List<StockAnnualReviewDto>> GetDataAsync(string stockId)
        {
            Query dividendQuery = new Query("StockDividend")
                .Where("StockId", stockId);

            List<StockDividend> dividends = await _repo.GetDividendByQueryAsync(dividendQuery);

            Query priceQuery = StockDailyPriceQueryBuilder.Build(
                stockId, null, "20000101", "20501231");

            List<StockDailyPrice> prices = (await _repo.GetPriceByQueryAsync(priceQuery))
                .OrderBy(x => x.TradeDate)
                .ToList();

            List<StockDailyPrice> adjPrices = ApplyAdjustments(prices,
                new List<SplitEvent>() {
                    new SplitEvent(){ 
                        StockId = "0050",
                        ExDate = new DateTime(2025, 6, 10),
                        Ratio = 4.0m,
                        Description = "0050 1拆4分割"
                    } 
                });

            IOrderedEnumerable<int> years = adjPrices
                .Select(p => p.TradeDate.Year)
                .Distinct()
                .OrderBy(y => y);

            List<StockAnnualReviewDto> result = new List<StockAnnualReviewDto>();
            foreach (var year in years)
            {
                var yearPrices = adjPrices
                    .Where(p => p.TradeDate.Year == year)
                    .OrderBy(p => p.TradeDate)
                    .ToList();

                if (yearPrices.Count == 0)
                    continue;

                List<StockDividend> yearDividends = dividends
                    .Where(d => d.Date.Year == year)
                    .ToList();

                decimal startOpen = yearPrices.First().OpenPrice;
                decimal endClose = yearPrices.Last().ClosePrice;

                decimal? cashDividend = yearDividends.Sum(d => d.CashEarningsDistribution);
                decimal? stockDividend = yearDividends.Sum(d => d.StockEarningsDistribution);

                decimal capitalGainRate = (endClose - startOpen) / startOpen * 100m;
                decimal? totalReturnRate = ((endClose - startOpen) + cashDividend) / startOpen * 100m;

                result.Add(new StockAnnualReviewDto
                {
                    Year = year,
                    StartOpen = startOpen,
                    EndClose = endClose,
                    CashDividend = cashDividend.Value,
                    StockDividend = stockDividend.Value,
                    CapitalGainRate = capitalGainRate,
                    TotalReturnRate = totalReturnRate.Value
                });
            }

            return result.OrderByDescending(x => x.Year).ToList();
        }
        private List<StockDailyPrice> ApplyAdjustments(List<StockDailyPrice> prices, List<SplitEvent> splits)
        {
            List<StockDailyPrice> newPrices = new List<StockDailyPrice>();
            // 1. 確保價格從「最新」到「最舊」排序 (由近到遠回溯)
            var sortedPrices = prices.OrderByDescending(p => p.TradeDate).ToList();

            // 2. 確保分割事件也從「最新」到「最舊」排序
            var sortedSplits = splits.OrderByDescending(s => s.ExDate).ToList();

            decimal cumulativeFactor = 1.0m;
            int splitIdx = 0;

            Console.WriteLine($"splits.Count {sortedSplits.Count} splitIdx {splitIdx} {sortedSplits[splitIdx].StockId}");

            foreach (var p in sortedPrices)
            {
                // 3. 如果當前的股價日期「早於」分割日，代表受到該次分割影響
                while (splitIdx < sortedSplits.Count && p.StockId == sortedSplits[splitIdx].StockId &&  p.TradeDate < sortedSplits[splitIdx].ExDate)
                {
                    // 累乘倍數（例如：如果經歷兩次 1 拆 2，這裡會變 2 * 2 = 4）
                    cumulativeFactor *= sortedSplits[splitIdx].Ratio;
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
                    PriceChange = p.PriceChange,
                    TradeCount = p.TradeCount,
                    Note = p.Note,
                });
            }
            return newPrices;
        }
    }

    public class SplitEvent
    {
        public string StockId { get; set; }
        /// <summary>
        /// 除權日（分割基準日）。在此日期「之前」的股價都需要進行還原調整。
        /// </summary>
        public DateTime ExDate { get; set; }

        /// <summary>
        /// 分割倍數。
        /// 例如：1 拆 4，倍數就是 4.0。
        /// 例如：台股配發 2 元股票股利（每 1000 股配 200 股），倍數就是 1.2。
        /// </summary>
        public decimal Ratio { get; set; }

        /// <summary>
        /// 選擇性欄位：備註（例如："0050 股票分割 1 拆 4"）
        /// </summary>
        public string Description { get; set; }
    }

}
