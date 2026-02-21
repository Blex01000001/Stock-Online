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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<StockUpdateHub> _hub;

        public UpdateOrchestrator(
            IServiceScopeFactory scopeFactory,
            IHubContext<StockUpdateHub> hub)
        {
            _scopeFactory = scopeFactory;
            _hub = hub;
        }

        public async Task<string> QueueUpdateJobAsync(UpdateCommand command)
        {
            var jobId = Guid.NewGuid().ToString();

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var sp = scope.ServiceProvider;

                    // ✅ 在「新 scope」取得這次要用的服務（避免用到 request scope 的物件）
                    var stockProvider = sp.GetRequiredService<IStockProvider>();
                    var updaters = sp.GetRequiredService<IEnumerable<IDataUpdater>>();
                    var updaterMap = updaters.ToDictionary(x => x.DataType);

                    // 1) 股票清單（如果 GetStockIdsAsync 真的回傳 Task，這裡就 await）
                    var stockIds = stockProvider.GetStockIdsAsync(command.Scope, command.SpecificStockId);

                    // 2) 年份清單（用你的原邏輯）
                    var years = GetYears(command.Time, command.SpecificYear);

                    await ProcessUpdateAsync(jobId, updaterMap, stockIds, years, command.TargetDataTypes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    await _hub.Clients.All.SendAsync("Progress", $"❌ 任務失敗 {jobId}：{ex.Message}");
                }
            });

            return jobId;
        }

        private async Task ProcessUpdateAsync(
            string jobId,
            IReadOnlyDictionary<DataType, IDataUpdater> updaterMap,
            List<string> stocks,
            List<int> years,
            List<DataType> dataTypes)
        {
            foreach (var dataType in dataTypes)
            {
                if (!updaterMap.TryGetValue(dataType, out var updater))
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
                $"✅ 任務完成 {jobId} 共 {stocks.Count} 檔股票，{years.Count} 年，{dataTypes.Count} 種資料類型"
            );
        }

        private List<int> GetYears(TimeScope scope, int? specificYear)
        {
            if (scope == TimeScope.SpecificYear)
                return new List<int> { specificYear ?? DateTime.Now.Year };

            return Enumerable.Range(1911, DateTime.Now.Year - 1911 + 1).ToList();
        }
    }
}
