using Microsoft.AspNetCore.SignalR;
using SqlKata;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Enums;
using Stock_Online.DTOs;
using Stock_Online.Hubs;
using Stock_Online.Services.KLine.Queries;
using Stock_Online.Services.Adjustment;

namespace Stock_Online.Services.AnnualReview
{
    public class StockAnnualReviewService : IStockAnnualReviewService
    {
        private readonly IStockRepository _repo;
        private readonly IPriceAdjustmentService _priceAdj;
        private readonly IDividendAdjustmentService _dividendAdj;

        private readonly IHubContext<StockUpdateHub> _hub;

        public StockAnnualReviewService(IStockRepository stockRepository, 
            IHubContext<StockUpdateHub> hub, 
            IPriceAdjustmentService priceAdjustmentService, 
            IDividendAdjustmentService dividendAdjustmentService)
        {
            _repo = stockRepository;
            _hub = hub;
            _priceAdj = priceAdjustmentService;
            _dividendAdj = dividendAdjustmentService;
        }
        public async Task<List<StockAnnualReviewDto>> GetDataAsync(string stockId)
        {
            List<StockCorporateAction> actions = await _repo.GetCorporateActionsAsync(stockId);

            Query dividendQuery = new Query("StockDividend")
                .Where("StockId", stockId);
            List<StockDividend> dividends = await _repo.GetDividendByQueryAsync(dividendQuery);
            IReadOnlyList<StockDividend> adjDividends = _dividendAdj.AdjustDividends(dividends, actions);

            Query priceQuery = StockDailyPriceQueryBuilder.Build(
                stockId, null, "20000101", "20501231");
            List<StockDailyPrice> prices = (await _repo.GetPriceByQueryAsync(priceQuery))
                .OrderBy(x => x.TradeDate)
                .ToList();
            IReadOnlyList<StockDailyPrice> adjPrices = _priceAdj.AdjustPrices(prices, actions);

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

                List<StockDividend> yearDividends = adjDividends
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
    }
}
