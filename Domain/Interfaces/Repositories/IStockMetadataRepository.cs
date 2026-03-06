using Stock_Online.Domain.Entities.Stock_Online.DTOs;

namespace Stock_Online.Domain.Interfaces.Repositories
{
    public interface IStockMetadataRepository
    {
        Task<List<StockInfoDto>> GetStockListAsync();
        Task<StockInfoDto> GetStockInfoAsync(string stockId);
        Task UpsertStockListAsync(List<StockInfoDto> stocks);
    }
}
