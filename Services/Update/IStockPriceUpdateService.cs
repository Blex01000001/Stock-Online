using Stock_Online.Domain.Entities;

namespace Stock_Online.Services.Update
{
    public interface IStockPriceUpdateService
    {
        Task<List<StockDailyPrice>> GetDailyPricesAsync(string stockId);
        Task FetchAndSaveAsync(int year, string stockId);
        Task FetchAndSaveAllStockAsync(int year);
    }
}
