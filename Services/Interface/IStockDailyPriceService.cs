using Stock_Online.Domain.Entities;

namespace Stock_Online.Services.Interface
{
    public interface IStockDailyPriceService
    {
        Task<List<StockDailyPrice>> GetDailyPricesAsync(string stockId);
        Task FetchAndSaveAsync(int year, string stockId);
    }
}
