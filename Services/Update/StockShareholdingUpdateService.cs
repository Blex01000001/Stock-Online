using Microsoft.AspNetCore.SignalR;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using Stock_Online.Hubs;
using System.Text.Json;

namespace Stock_Online.Services.Update
{
    public sealed class StockShareholdingUpdateService : IStockShareholdingUpdateService
    {
        private readonly HttpClient _http;
        private readonly IStockRepository _repo;
        private readonly IHubContext<StockUpdateHub> _hub;

        private static readonly JsonSerializerOptions JsonOpt = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public StockShareholdingUpdateService(
            IStockRepository repo,
            IHubContext<StockUpdateHub> hub
        )
        {
            _http = new HttpClient();
            _repo = repo;
            _hub = hub;
            //_getAllStockIdsAsync = getAllStockIdsAsync;
            //_finMindToken = finMindToken;
        }

        public async Task FetchAndSaveAsync(string stockId)
        {
            var startDate = "1911-01-01"; //(FinMind 用 yyyy-MM-dd)
            try{
                await ReportProgressAsync($"更新中 {stockId}");
                var count = await FetchAndSaveAsync(stockId, startDate);
                await ReportProgressAsync($"✅ 更新完成 {stockId}");
            }
            catch (Exception ex){
                await ReportProgressAsync($"❌ 更新失敗 {stockId} {ex.Message}");
            }
        }

        public async Task<int> FetchAndSaveAsync(string stockId, string startDate)
        {
            //  API 
            // https://api.finmindtrade.com/api/v4/data?dataset=TaiwanStockShareholding&data_id=2330&start_date=2026-02-01
            var url =
                $"https://api.finmindtrade.com/api/v4/data" +
                $"?dataset=TaiwanStockShareholding" +
                $"&data_id={Uri.EscapeDataString(stockId)}" +
                $"&start_date={Uri.EscapeDataString(startDate)}";
            using var resp = await _http.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            var payload = await resp.Content.ReadFromJsonAsync<FinMindResponse<TaiwanStockShareholdingDto>>(JsonOpt);
            if (payload == null) return 0;
            if (payload.Status != 200) return 0;
            if (payload.Data == null || payload.Data.Count == 0) return 0;

            var entities = payload.Data.Select(d => new StockShareholding
            {
                StockId = d.stock_id,
                Date = d.date, // 這邊我直接存 yyyy-MM-dd；若你 DB 全部都用 yyyyMMdd，請在此轉換

                StockName = d.stock_name,
                InternationalCode = d.InternationalCode,

                ForeignInvestmentRemainingShares = d.ForeignInvestmentRemainingShares,
                ForeignInvestmentShares = d.ForeignInvestmentShares,
                ForeignInvestmentRemainRatio = d.ForeignInvestmentRemainRatio,
                ForeignInvestmentSharesRatio = d.ForeignInvestmentSharesRatio,
                ForeignInvestmentUpperLimitRatio = d.ForeignInvestmentUpperLimitRatio,
                ChineseInvestmentUpperLimitRatio = d.ChineseInvestmentUpperLimitRatio,
                NumberOfSharesIssued = d.NumberOfSharesIssued,

                RecentlyDeclareDate = string.IsNullOrWhiteSpace(d.RecentlyDeclareDate) ? null : d.RecentlyDeclareDate,
                Note = string.IsNullOrWhiteSpace(d.note) ? null : d.note
            }).ToList();

            await _repo.SaveShareholdingToDb(entities);
            return entities.Count;
        }
        private async Task ReportProgressAsync(string message)
        {
            await _hub.Clients.All.SendAsync("Progress", message);
        }

    }
}
