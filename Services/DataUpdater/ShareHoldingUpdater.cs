using Microsoft.AspNetCore.SignalR;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using Stock_Online.DTOs.UpdateRequest;
using Stock_Online.Hubs;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace Stock_Online.Services.DataUpdater
{
    public class ShareHoldingUpdater : IDataUpdater
    {
        private readonly HttpClient _http;

        private readonly IStockRepository _repo;
        private readonly IHubContext<StockUpdateHub> _hub;

        public DataType DataType => DataType.ShareHoldingUpdater;
        private static readonly JsonSerializerOptions JsonOpt = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ShareHoldingUpdater(IStockRepository repo, IHubContext<StockUpdateHub> hub)
        {
            _repo = repo;
            _hub = hub;
            _http = new HttpClient();
        }
        public async   Task UpdateAsync(string stockId, int year)
        {
            var startDate = $"{year}-01-01"; //(FinMind 用 yyyy-MM-dd)
            try
            {
                await ReportProgressAsync($"更新中 {stockId}");
                var count = await FetchAndSaveAsync(stockId, startDate);
                await ReportProgressAsync($"✅ 更新完成 {stockId}");
            }
            catch (Exception ex)
            {
                await ReportProgressAsync($"❌ 更新失敗 {stockId} {ex.Message}");
            }
        }

        public async Task<int> FetchAndSaveAsync(string stockId, string startDate)
        {
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
                Date = d.date,

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
