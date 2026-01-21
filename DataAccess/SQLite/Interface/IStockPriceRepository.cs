using SqlKata;
using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Entities.Stock_Online.DTOs;

namespace Stock_Online.DataAccess.SQLite.Interface
{
    public interface IStockPriceRepository
    {
        Task<List<StockDailyPrice>> GetByStockIdAsync(string stockId);
        Task<List<StockDailyPrice>> GetByQueryAsync(Query query);
        Task<StockInfoDto?> GetStockInfoAsync(string stockId);
        Task<List<string>> GetAllStockIdsAsync();
        void SaveToDb(List<StockDailyPrice> list);
    }
}
