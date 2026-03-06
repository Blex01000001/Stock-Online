using SqlKata;
using Stock_Online.Domain.Entities;

namespace Stock_Online.Domain.Interfaces.Repositories
{
    public interface ITradeDataRepository
    {
        Task<List<StockDailyPrice>> GetDailyPricesAsync(Query query);
        Task UpsertDailyPricesAsync(List<StockDailyPrice> prices);
        Task<List<StockDividend>> GetDividendsAsync(Query query);
        Task UpsertDividendsAsync(List<StockDividend> dividends);
        Task<List<StockCorporateAction>> GetCorporateActionsAsync(Query query);
        Task UpsertCorporateActionsAsync(List<StockCorporateAction> actions);
    }
}
