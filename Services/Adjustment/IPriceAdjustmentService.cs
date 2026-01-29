using Stock_Online.Domain.Entities;

namespace Stock_Online.Services.Adjustment
{
    public interface IPriceAdjustmentService
    {
        IReadOnlyList<StockDailyPrice> AdjustPrices(
            IReadOnlyList<StockDailyPrice> prices,
            IReadOnlyList<StockCorporateAction> actions);
    }
}
