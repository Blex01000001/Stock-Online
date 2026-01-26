using Microsoft.AspNetCore.SignalR;
using Stock_Online.DTOs;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.Hubs;
using System;

namespace Stock_Online.Services.Update
{
    public class StockDividendUpdateService : IStockDividendUpdateService
    {
        private readonly IStockPriceRepository _repo;
        private readonly IHubContext<StockUpdateHub> _hub;

        public StockDividendUpdateService(IStockPriceRepository repo, IHubContext<StockUpdateHub> hub)
        {
            _repo = repo;
            _hub = hub;
        }
        public async Task FetchAndSaveAllStockAsync()
        {
            var startTime = DateTime.Now;
            var stockIds = await _repo.GetAllStockIdsAsync();
            //List<string> stockIds = stockIds1.Skip(1010).ToList();
            int total = stockIds.Count;
            int success = 0;
            int fail = 0;

            using var http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            for (int i = 0; i < stockIds.Count; i++)
            {
                var stockId = stockIds[i];
                int current = i + 1;

                try
                {

                    await ReportProgressAsync(
                        $"股利更新中 {stockId} ({current}/{total})"
                    );

                    string url =
                        "https://api.finmindtrade.com/api/v4/data" +
                        "?dataset=TaiwanStockDividend" +
                        $"&data_id={stockId}" +
                        "&start_date=2010-01-01";
                    Console.WriteLine($"Update URL: {url}");

                    var response =
                        await http.GetFromJsonAsync<FinMindDividendResponse>(url);

                    if (response == null ||
                        response.status != 200 ||
                        response.data == null ||
                        response.data.Count == 0)
                    {
                        fail++;
                        await ReportProgressAsync(
                            $"⚠ 股利資料為空 {stockId} ({current}/{total})"
                        );
                        continue;
                    }

                    var models = response.data
                        .Select(x => MapToEntity(x))
                        .Where(x => x != null)
                        .Cast<StockDividend>()
                        .ToList();

                    _repo.SaveDividendToDb(models);

                    success++;

                    await ReportProgressAsync(
                        $"✅ 更新完成 {stockId} ({current}/{total})"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Update URL: {ex}");
                    fail++;
                    await ReportProgressAsync(
                        $"❌ 更新失敗 {stockId} ({current}/{total})"
                    );
                }

                // 🧠 禮貌 delay（FinMind 有 rate limit）
                await Task.Delay(300);
            }

            var elapsed = DateTime.Now - startTime;

            await ReportProgressAsync(
                $"🎉 更新結束 成功:{success} 失敗:{fail} 共:{total}  耗時:{elapsed:mm\\:ss}"
            );




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
