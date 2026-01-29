using Stock_Online.Domain.Entities;

namespace Stock_Online.Services.Adjustment
{
    public interface IDividendAdjustmentService
    {
        IReadOnlyList<StockDividend> AdjustDividends(
            IReadOnlyList<StockDividend> dividends,
            IReadOnlyList<StockCorporateAction> actions);
    }
}
