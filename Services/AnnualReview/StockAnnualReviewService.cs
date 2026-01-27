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
            var dividendQuery = new Query("StockDividend")
                .Where("StockId", stockId);

            var dividends = await _repo.GetDividendByQueryAsync(dividendQuery);

            var priceQuery = StockDailyPriceQueryBuilder.Build(
                stockId, null, "20000101", "20501231");

            var prices = (await _repo.GetPriceByQueryAsync(priceQuery))
                .OrderBy(x => x.TradeDate)
                .ToList();

            var result = new List<StockAnnualReviewDto>();

            var years = prices
                .Select(p => p.TradeDate.Year)
                .Distinct()
                .OrderBy(y => y);

            foreach (var year in years)
            {
                var yearPrices = prices
                    .Where(p => p.TradeDate.Year == year)
                    .OrderBy(p => p.TradeDate)
                    .ToList();

                if (yearPrices.Count == 0)
                    continue;

                var yearDividends = dividends
                    .Where(d => d.Date.Year == year)
                    .ToList();

                var startOpen = yearPrices.First().OpenPrice;
                var endClose = yearPrices.Last().ClosePrice;

                var cashDividend = yearDividends.Sum(d => d.CashEarningsDistribution);
                var stockDividend = yearDividends.Sum(d => d.StockEarningsDistribution);

                var capitalGainRate =
                    (endClose - startOpen) / startOpen * 100m;

                var totalReturnRate =
                    ((endClose - startOpen) + cashDividend) / startOpen * 100m;

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

        //public async void GetData(string stockId)
        //{
        //    var dividendQuery = new Query("StockDividend")
        //        .Where("StockId", stockId)
        //        .OrderBy("Date");

        //    List<StockDividend> dividends = await _repo.GetDividendByQueryAsync(dividendQuery);

        //    Query priceQuery = StockDailyPriceQueryBuilder.Build(stockId, null, "20000101", "20501231");
        //    List<StockDailyPrice> prices = (await _repo.GetPriceByQueryAsync(priceQuery))
        //        .OrderBy(x => x.TradeDate)
        //        .ToList();

        //    int[] years = Enumerable.Range(2000, DateTime.Now.Year - 2000 + 1).ToArray();
        //    foreach (var year in years)
        //    {
        //        List<StockDailyPrice> yearPrices = prices.Where(x => x.TradeDate.Year == year).OrderBy(x => x.TradeDate).ToList();
        //        List<StockDividend> yearDividends = dividends.Where(x => x.Date.Year == year).OrderBy(x => x.Date).ToList();

        //        if (yearPrices.Count == 0 || yearDividends.Count == 0) 
        //            continue;

        //        var firstDay = yearPrices[0].OpenPrice;
        //        var lastDay = yearPrices[yearPrices.Count - 1].ClosePrice;
        //        var rate = (lastDay - firstDay) / firstDay * 100;
        //        var totalCash = yearDividends.Sum(x => x.CashEarningsDistribution);
        //        var rateWithCash = ((lastDay - firstDay) + totalCash) / firstDay * 100;

        //    }

        //}
    }
}
