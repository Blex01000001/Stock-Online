using Microsoft.AspNetCore.SignalR;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using Stock_Online.DTOs.UpdateRequest;
using Stock_Online.Hubs;

namespace Stock_Online.Services.DataUpdater
{
    public class DividendUpdater : IDataUpdater
    {
        private readonly IStockRepository _repo;
        private readonly IHubContext<StockUpdateHub> _hub;

        public DataType DataType => DataType.DividendUpdater;

        public DividendUpdater(IStockRepository repo, IHubContext<StockUpdateHub> hub)
        {
            _repo = repo;
            _hub = hub;
        }
        public async Task UpdateAsync(string stockId, int year)
        {
            using var http = new HttpClient{ Timeout = TimeSpan.FromSeconds(15)};
            try
            {
                await ReportProgressAsync($"{stockId} 股利更新中");

                string url =
                    "https://api.finmindtrade.com/api/v4/data" +
                    "?dataset=TaiwanStockDividend" +
                    $"&data_id={stockId}" +
                    $"&start_date={year}-01-01";

                var response =await http.GetFromJsonAsync<FinMindDividendResponse>(url);

                if (response == null ||
                    response.status != 200 ||
                    response.data == null ||
                    response.data.Count == 0)
                {
                    await ReportProgressAsync(
                        $"⚠ 股利資料為空 {stockId}"
                    );
                }

                var models = response.data
                    .Select(x => MapToEntity(x))
                    .Where(x => x != null)
                    .Cast<StockDividend>()
                    .ToList();

                _repo.SaveDividendToDb(models);

                await ReportProgressAsync(
                    $"✅ 股利更新完成 {stockId} "
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update URL: {ex}");
                await ReportProgressAsync(
                    $"❌ 股利更新失敗 {stockId} "
                );
            }

            // 🧠 禮貌 delay（FinMind 有 rate limit）
            await Task.Delay(300);
        }
        private async Task ReportProgressAsync(string message)
        {
            await _hub.Clients.All.SendAsync("Progress", message);
        }
        private static StockDividend? MapToEntity(FinMindDividendDto dto)
        {
            try
            {
                return new StockDividend
                {
                    StockId = dto.stock_id,
                    Date = DateTime.Parse(dto.date),
                    Year = dto.year,

                    StockEarningsDistribution = dto.StockEarningsDistribution,
                    StockStatutorySurplus = dto.StockStatutorySurplus,
                    StockExDividendTradingDate = EmptyToNull(dto.StockExDividendTradingDate),

                    TotalEmployeeStockDividend = dto.TotalEmployeeStockDividend,
                    TotalEmployeeStockDividendAmount = dto.TotalEmployeeStockDividendAmount,
                    RatioOfEmployeeStockDividendOfTotal = dto.RatioOfEmployeeStockDividendOfTotal,
                    RatioOfEmployeeStockDividend = dto.RatioOfEmployeeStockDividend,

                    CashEarningsDistribution = dto.CashEarningsDistribution,
                    CashStatutorySurplus = dto.CashStatutorySurplus,
                    CashExDividendTradingDate = EmptyToNull(dto.CashExDividendTradingDate),
                    CashDividendPaymentDate = EmptyToNull(dto.CashDividendPaymentDate),

                    TotalEmployeeCashDividend = dto.TotalEmployeeCashDividend,
                    TotalNumberOfCashCapitalIncrease = dto.TotalNumberOfCashCapitalIncrease,
                    CashIncreaseSubscriptionRate = dto.CashIncreaseSubscriptionRate,
                    CashIncreaseSubscriptionPrice = dto.CashIncreaseSubscriptionpRrice,

                    RemunerationOfDirectorsAndSupervisors =
                        dto.RemunerationOfDirectorsAndSupervisors,

                    ParticipateDistributionOfTotalShares =
                        dto.ParticipateDistributionOfTotalShares,

                    AnnouncementDate = dto.AnnouncementDate,
                    AnnouncementTime = dto.AnnouncementTime
                };
            }
            catch
            {
                return null;
            }
        }

        private static string? EmptyToNull(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s;

    }
}
