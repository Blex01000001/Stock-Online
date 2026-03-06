using Stock_Online.Application.DTOs.Commands;
using Stock_Online.Domain.Interfaces.Repositories;
using System.Collections.ObjectModel;

namespace Stock_Online.Application.Services.StockProvider
{
    public class StockProvider : IStockProvider
    {
        private readonly IStockRepository _repo;
        // 使用 ReadOnlyDictionary 確保資料初始化後不會被意外修改，這在 Parallel 環境下是絕對安全的
        private ReadOnlyDictionary<string, string> _stockMap;
        public List<string> GetStockIdsAsync(StockScope scope, string? singleId)
        {
            if (scope == StockScope.Single)
                return new List<string> { singleId };

            return new List<string> { "2330", "9933", "1101" };
        }
        public StockProvider(IStockRepository repo)
        {
            _repo = repo;
            Initialize();
        }

        private void Initialize()
        {
            var allStocks = _repo.GetStockInfosAsync().GetAwaiter().GetResult();
            var dict = allStocks.ToDictionary(x => x.StockId, x => x.CompanyShortName);
            _stockMap = new ReadOnlyDictionary<string, string>(dict);
        }

        public string GetName(string stockId) => _stockMap.GetValueOrDefault(stockId, stockId);

        public bool TryGetName(string stockId, out string name) => _stockMap.TryGetValue(stockId, out name);

    }
}
