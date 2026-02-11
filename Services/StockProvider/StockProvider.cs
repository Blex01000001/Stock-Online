using Stock_Online.DTOs.UpdateRequest;

namespace Stock_Online.Services.StockProvider
{
    public class StockProvider : IStockProvider
    {
        public List<string> GetStockIdsAsync(StockScope scope, string? singleId)
        {
            if (scope == StockScope.Single)
                return new List<string> { singleId };

            return new List<string> { "2330", "9933", "1101" };


        }
    }
}
