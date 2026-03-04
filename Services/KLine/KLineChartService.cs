using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Services.KLine.Builders;
using Stock_Online.Services.KLine.Queries;
using SqlKata;
using Stock_Online.Domain.Entities;
using Stock_Online.Services.Adjustment;
using Stock_Online.Services.KLine.Models.DTOs;
using Stock_Online.Services.StockProvider;
using Microsoft.AspNetCore.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Stock_Online.Services.KLine
{
    public class KLineChartService : IKLineChartService
    {
        private readonly IStockRepository _repo;
        private readonly IPriceAdjustmentService _priceAdjService;
        private readonly IInstitutionalInvestorsAdjustmentService _insInvAdjService;
        private readonly IStockProvider _stockProvider;

        public KLineChartService(IStockRepository repo,
            IPriceAdjustmentService priceAdjService,
            IInstitutionalInvestorsAdjustmentService insInvAdjService,
            IStockProvider stockProvider)
        {
            this._repo = repo;
            this._priceAdjService = priceAdjService;
            this._insInvAdjService = insInvAdjService;
            this._stockProvider = stockProvider;
        }

        public async Task<List<KLineChartDto>> GetKLineAsync(string stockId, int? days, string? start, string? end)
        {
            var stockName = _stockProvider.GetName(stockId);
            var actionQuery = new Query("StockCorporateAction")
                .Select("StockId", "ActionType", "ExDate", "Ratio", "CashAmount", "Description")
                .Where("StockId", stockId)
                .OrderByDesc("ExDate");
            List<StockCorporateAction> actions = await _repo.GetCorporateActionsAsync(actionQuery);


            // 正常期間
            Query priceQuery = StockDailyPriceQueryBuilder.Build(stockId, null, start, end);
            List<StockDailyPrice> prices = (await _repo.GetPricesAsync(priceQuery))
                .OrderBy(x => x.TradeDate)
                .ToList();



            // 僅抓取必要長度：目標起始日往前推 240 天(最大 MA) 即可滿足多數指標計算
            DateTime startDate = DateTime.ParseExact(start, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            //DateTime startDate = DateTime.TryParse(start, out var s) ? s : DateTime.Now.AddYears(-1);
            string effectiveStart = startDate.AddDays(-365).ToString("yyyyMMdd");

            Query priceQueryRequired = StockDailyPriceQueryBuilder.Build(stockId, null, effectiveStart, end);
            var pricesRequired = (await _repo.GetPricesAsync(priceQueryRequired))
                .OrderBy(x => x.TradeDate)
                .ToList();
            var adjPricesAll = _priceAdjService.AdjustPrices(pricesRequired, actions);

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
            var adjInsInvBuySells = _insInvAdjService.AdjustBuySell(insInvBuySells, actions).ToList();

            return new List<KLineChartDto> { 
                new KLineChartBuilder(stockId, stockName, adjPricesAll, index)
                    .SetMaLine()
                    .SetBollingerBand()
                    .SetForeignInvestmentHolding(StockShareholdings)
                    .SetInstitutional(adjInsInvBuySells)
                    .SetMacd()
                    .SetRsi()
                    .Build()
            };
        }
    }
}
