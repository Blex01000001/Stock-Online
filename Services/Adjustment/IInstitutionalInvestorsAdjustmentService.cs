using Stock_Online.Domain.Entities;

namespace Stock_Online.Services.Adjustment
{
    public interface IInstitutionalInvestorsAdjustmentService
    {
        IReadOnlyList<StockInstitutionalInvestorsBuySell> AdjustBuySell(
            IReadOnlyList<StockInstitutionalInvestorsBuySell> buySellData,
            IReadOnlyList<StockCorporateAction> actions);
    }
}
