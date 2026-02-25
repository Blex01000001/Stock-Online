using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.DTOs;
using Stock_Online.Services.KLine.Builders;
using Stock_Online.Services.KLine.Queries;
using SqlKata;
using Stock_Online.Domain.Entities;
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

            return new List<KLineChartDto> { new KLineChartBuilder(stockId, adjPricesAll, index)
                .SetHolding(StockShareholdings)
                .SetInstitutional(adjInsInvBuySells.ToList())
                .CreateSingle()
            };
        }
    }
}
