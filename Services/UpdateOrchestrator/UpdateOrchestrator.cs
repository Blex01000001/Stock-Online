using Stock_Online.DTOs.UpdateRequest;
using Stock_Online.Services.StockProvider;
using Stock_Online.Services.DataUpdater;
using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using Stock_Online.Hubs;


namespace Stock_Online.Services.UpdateOrchestrator
{
    public class UpdateOrchestrator : IUpdateOrchestrator
    {
        //private readonly IEnumerable<IDataUpdater> _updaters; // 注入所有實作了 IDataUpdater 的類別
        private readonly IReadOnlyDictionary<DataType, IDataUpdater> _updaterMap;
        private readonly IStockProvider _stockProvider;       // 專門負責抓取單一/常用/全部股票清單
        private readonly IHubContext<StockUpdateHub> _hub;
        public UpdateOrchestrator(IStockProvider stockProvider, IEnumerable<IDataUpdater> updaters, IHubContext<StockUpdateHub> hub)
        {
            this._stockProvider = stockProvider;
            this._updaterMap = updaters.ToDictionary(x => x.DataType);
            this._hub = hub;
        }
        public async Task<string> QueueUpdateJobAsync(UpdateCommand command)
        {
            // 1. 決定股票清單
            var stockIds = _stockProvider.GetStockIdsAsync(command.Scope, command.SpecificStockId);

            // 2. 決定年份清單
            var years = GetYears(command.Time, command.SpecificYear);

            // 3. 執行更新 (這裡可以是非同步 Task.Run 或丟入 RabbitMQ/Hangfire)
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessUpdateAsync(stockIds, years, command.TargetDataTypes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });

            return Guid.NewGuid().ToString();
        }
        private async Task ProcessUpdateAsync(List<string> stocks, List<int> years, List<DataType> dataTypes)
        {
            foreach (var dataType in dataTypes)
            {
                if (!_updaterMap.TryGetValue(dataType, out var updater))
                {
                    Console.WriteLine($"找不到 IDataUpdater for {dataType}");
                    continue;
                }

                foreach (var stock in stocks)
                    foreach (var year in years)
                    {
                        Console.WriteLine($"{dataType} {stock} {year}");
                        await updater.UpdateAsync(stock, year);
                    }
            }
            await _hub.Clients.All.SendAsync(
                "Progress",
                $"✅ 任務完成：共 {stocks.Count} 檔股票，{years.Count} 年，{dataTypes.Count} 種資料類型"
            );
        }
        private List<int> GetYears(TimeScope scope, int? specificYear)
        {
            if (scope == TimeScope.SpecificYear) return new List<int> { specificYear ?? DateTime.Now.Year };
            return Enumerable.Range(1911, DateTime.Now.Year - 1911 + 1).ToList();
        }
    }
}
