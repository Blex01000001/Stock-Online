using Stock_Online.Application.DTOs.Commands;

namespace Stock_Online.Application.Services.StockProvider
{
    public interface IStockProvider
    {
        // 傳入 Scope，回傳 List<string> { "2330", "2317", ... }
        List<string> GetStockIdsAsync(StockScope scope, string? singleId);
        string GetName(string stockId);
        bool TryGetName(string stockId, out string name);
    }
}
