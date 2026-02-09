using SqlKata;
using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Entities.Stock_Online.DTOs;

namespace Stock_Online.DataAccess.SQLite.Interface
{
    public interface IStockRepository
    {
        Task<List<StockDailyPrice>> GetPriceByStockIdAsync(string stockId);
        Task<List<StockDailyPrice>> GetPriceByQueryAsync(Query query);
        Task<List<StockDividend>> GetDividendByQueryAsync(Query query);
        Task<List<StockCorporateAction>> GetCorporateActionsAsync(string stockId);
        Task<StockInfoDto?> GetStockInfoAsync(string stockId);
        Task<List<string>> GetAllStockIdsAsync();
        void SaveToDb(List<StockDailyPrice> list);
        void SaveDividendToDb(List<StockDividend> list);
        Task SaveShareholdingToDb(List<StockShareholding> list);
    }
}
