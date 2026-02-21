using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.DTOs;
using Stock_Online.Services.KLine.Builders;
using Stock_Online.Services.KLine.Indicators;
using Stock_Online.Services.KLine.Queries;
using Stock_Online.Services.KLine.Patterns;
using SqlKata;
using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Enums;
using System.Collections.Generic;
using Stock_Online.Services.Adjustment;

namespace Stock_Online.Services.KLine
{
    public class KLineChartService : IKLineChartService
    {
        private readonly IStockRepository _repo;
        private readonly IPriceAdjustmentService _priceAdjService;
        private readonly IInstitutionalInvestorsAdjustmentService _insInvAdjService;


        public KLineChartService(IStockRepository repo,
            IPriceAdjustmentService priceAdjService,
            IInstitutionalInvestorsAdjustmentService insInvAdjService)
        {
            this._repo = repo;
            this._priceAdjService = priceAdjService;
            this._insInvAdjService = insInvAdjService;
        }

        public async Task<List<KLineChartDto>> GetKLineAsync(
            string stockId,
            int? days,
            string? start,
            string? end
        )
        {
            var actionQuery = new Query("StockCorporateAction")
                .Select("StockId", "ActionType", "ExDate", "Ratio", "CashAmount", "Description")
                .Where("StockId", stockId)
                .OrderByDesc("ExDate");
            List<StockCorporateAction> actions = await _repo.GetCorporateActionsAsync(actionQuery);


            Query priceQuery = StockDailyPriceQueryBuilder.Build(stockId, null, start, end);
            List<StockDailyPrice> prices = (await _repo.GetPricesAsync(priceQuery))
                .OrderBy(x => x.TradeDate)
                .ToList();


            Query priceQueryAll = StockDailyPriceQueryBuilder.Build(stockId, null, "19110101", end);
            List<StockDailyPrice> pricesAll = (await _repo.GetPricesAsync(priceQueryAll))
                .OrderBy(x => x.TradeDate)
                .ToList();
            var adjPricesAll = _priceAdjService.AdjustPrices(pricesAll, actions);


            var allPriceDic = adjPricesAll.ToDictionary(k => k.TradeDate, v => v);
            int index = adjPricesAll.ToList().IndexOf(allPriceDic[prices[0].TradeDate]);

            Query ShareholdingQuery = new Query("StockShareholding").Where("StockId", stockId);
            List<StockShareholding> StockShareholdings = (await _repo.GetShareHoldingsAsync(ShareholdingQuery))
                .OrderBy(x => x.Date)
                .ToList();

            Query InstitutionalQuery = new Query("StockInstitutionalInvestorsBuySell").Where("StockId", stockId);
            List<StockInstitutionalInvestorsBuySell> insInvBuySells = (await _repo.GetInstitutionalInvestorsBuySellAsync(InstitutionalQuery))
                .OrderBy(x => x.Date)
                .ToList();
            var adjInsInvBuySells = _insInvAdjService.AdjustBuySell(insInvBuySells, actions);



            return new List<KLineChartDto> { new KLineChartBuilder(stockId, adjPricesAll, index, false)
                .SetHolding(StockShareholdings)
                .SetInstitutional(adjInsInvBuySells.ToList())
                .CreateSingle()
            };
        }
        public async Task<List<KLineChartDto>> GetKPatternLineAsync(string stockId, CandlePattern candlePattern)
        {
            Console.WriteLine($"Get K Line: {stockId}");
            Query query = StockDailyPriceQueryBuilder.Build(stockId, null, "20200101", "20261231");

            List<StockDailyPrice> prices = (await _repo.GetPricesAsync(query))
                .OrderBy(x => x.TradeDate)
                .ToList();

            Dictionary<int, List<decimal?>> maMap = MovingAverageCalculator.Calculate(prices);

            List<KLineChartDto> klines = new();

            for (int n = 0; n < prices.Count; n++)
            {
                var detector = new CandlePatternDetector(prices, n, maMap)
                    .IsMatch(candlePattern);
                if (!detector) continue;

                //只在「確定命中」後才切 specPrices（給前端）
                klines.Add(new KLineChartBuilder(stockId,prices,n).Create());
            }
            return klines;
        }
    }
}
