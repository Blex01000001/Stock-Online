using Stock_Online.Domain.Entities;

namespace Stock_Online.DataAccess.SQLite.Interface
{
    public interface IStockDailyPriceRepository
    {
        Task<List<StockDailyPrice>> GetByStockIdAsync(string stockId);
    }
}
