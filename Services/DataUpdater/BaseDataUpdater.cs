using Microsoft.AspNetCore.SignalR;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Hubs;
using Stock_Online.DTOs.UpdateRequest;

namespace Stock_Online.Services.DataUpdater
{
    public abstract class BaseDataUpdater : IDataUpdater
    {
        protected readonly IStockRepository _repo;
        protected readonly IHubContext<StockUpdateHub> _hub;
        protected readonly HttpClient _http;

        public abstract DataType DataType { get; }

        protected BaseDataUpdater(
            IStockRepository repo,
            IHubContext<StockUpdateHub> hub,
            HttpClient http)
        {
            this._repo = repo;
            this._hub = hub;
            this._http = http;
        }

        public abstract Task UpdateAsync(string stockId, int year);

        protected Task ReportProgressAsync(string message)
            => _hub.Clients.All.SendAsync("Progress", message);

        protected static string? EmptyToNull(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s;

        protected async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> action,
            Func<int, Exception, Task>? onRetryFail = null,
            int maxRetry = 3,
            int delaySeconds = 5)
        {
            for (int retry = 1; retry <= maxRetry; retry++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    if (onRetryFail != null)
                        await onRetryFail(retry, ex);

                    if (retry == maxRetry)
                        throw;

                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            throw new InvalidOperationException("ExecuteWithRetryAsync unreachable.");
        }
    }
}
