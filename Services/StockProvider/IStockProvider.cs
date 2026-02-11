using Stock_Online.DTOs.UpdateRequest;

namespace Stock_Online.Services.StockProvider
{
    public interface IStockProvider
    {
        // 傳入 Scope，回傳 List<string> { "2330", "2317", ... }
        List<string> GetStockIdsAsync(StockScope scope, string? singleId);
    }
}
