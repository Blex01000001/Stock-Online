using Stock_Online.Domain.Entities.Stock_Online.DTOs;

namespace Stock_Online.DataAccess.SQLite.Interface
{
    public interface IStockMetadataRepository
    {
        Task<List<StockInfoDto>> GetStockListAsync();
        Task<StockInfoDto> GetStockInfoAsync(string stockId);
        Task UpsertStockListAsync(List<StockInfoDto> stocks);
    }
}
