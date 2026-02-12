using Microsoft.AspNetCore.SignalR;
using static System.Net.WebRequestMethods;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using Stock_Online.Hubs;
using System.Text.Json;
using Stock_Online.DTOs.UpdateRequest;

namespace Stock_Online.Services.DataUpdater
{
    public class InstitutionalInvestorsBuySellUpdater : BaseDataUpdater
    {
        public override DataType DataType => DataType.InstitutionalInvestorsBuySellUpdater;

        private static readonly JsonSerializerOptions JsonOpt = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public InstitutionalInvestorsBuySellUpdater(
            IStockRepository repo,
            IHubContext<StockUpdateHub> hub,
            HttpClient http)
            : base(repo, hub, http)
        {
            _http.Timeout = TimeSpan.FromSeconds(15);
        }

        public override async Task UpdateAsync(string stockId, int year)
        {
            var startDate = $"{year}-01-01";

            try
            {
                await ReportProgressAsync($"三大法人買賣超更新中 {stockId}");

                var count = await ExecuteWithRetryAsync(
                    () => FetchAndSaveAsync(stockId, startDate),
                    onRetryFail: (retry, ex) =>
                        ReportProgressAsync($"⚠ 三大法人買賣超重試失敗 {stockId} 第{retry}次：{ex.Message}")
                );

                await ReportProgressAsync($"✅ 三大法人買賣超更新完成 {stockId}（{count}筆）");
            }
            catch (Exception ex)
            {
                await ReportProgressAsync($"❌ 三大法人買賣超更新失敗 {stockId}：{ex.Message}");
            }
        }

        private async Task<int> FetchAndSaveAsync(string stockId, string startDate)
        {
            var url =
                $"https://api.finmindtrade.com/api/v4/data" +
                $"?dataset=TaiwanStockInstitutionalInvestorsBuySell" +
                $"&data_id={Uri.EscapeDataString(stockId)}" +
                $"&start_date={Uri.EscapeDataString(startDate)}";

            using var resp = await _http.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            var payload = await resp.Content.ReadFromJsonAsync<FinMindResponse<InstitutionalInvestorsBuySellDto>>(JsonOpt);
            if (payload == null) return 0;
            if (payload.Status != 200) return 0;
            if (payload.Data == null || payload.Data.Count == 0) return 0;

            var entities = payload.Data.Select(d => new StockInstitutionalInvestorsBuySell
            {
                StockId = d.stock_id,
                Date = d.date,
                Name = d.name,
                Buy = d.buy,
                Sell = d.sell
            }).ToList();

            await _repo.SaveInstitutionalInvestorsBuySellToDb(entities);
            return entities.Count;
        }
    }
}
