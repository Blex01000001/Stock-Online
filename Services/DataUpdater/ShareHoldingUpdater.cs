using Microsoft.AspNetCore.SignalR;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using Stock_Online.DTOs.UpdateRequest;
using Stock_Online.Hubs;
using System.Text.Json;

namespace Stock_Online.Services.DataUpdater
{
    public class ShareHoldingUpdater : BaseDataUpdater
    {
        public override DataType DataType => DataType.ShareHoldingUpdater;

        private static readonly JsonSerializerOptions JsonOpt = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ShareHoldingUpdater(
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
                await ReportProgressAsync($"⏳ {stockId} {year} 外資持股更新中");

                var count = await ExecuteWithRetryAsync(
                    () => FetchAndSaveAsync(stockId, startDate),
                    onRetryFail: (retry, ex) =>
                        ReportProgressAsync($"⚠ {stockId} {year} 外資持股重試失敗 第{retry}次：{ex.Message}")
                );

                await ReportProgressAsync($"✅ {stockId} {year} 外資持股更新完成（{count}筆）");
            }
            catch (Exception ex)
            {
                await ReportProgressAsync($"❌ {stockId} {year} 外資持股更新失敗：{ex.Message}");
            }
        }

        private async Task<int> FetchAndSaveAsync(string stockId, string startDate)
        {
            var url =
                $"https://api.finmindtrade.com/api/v4/data" +
                $"?dataset=TaiwanStockShareholding" +
                $"&data_id={Uri.EscapeDataString(stockId)}" +
                $"&start_date={Uri.EscapeDataString(startDate)}";

            using var resp = await _http.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            var payload = await resp.Content.ReadFromJsonAsync<FinMindResponse<ShareholdingDto>>(JsonOpt);
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

                RecentlyDeclareDate = EmptyToNull(d.RecentlyDeclareDate),
                Note = EmptyToNull(d.note)
            }).ToList();

            await _repo.SaveShareholdingToDb(entities);
            return entities.Count;
        }
    }
}
