using System.Net.Http.Json;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using SqlKata;
using Stock_Online.DataAccess;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using Stock_Online.Hubs;
using Stock_Online.Services.KLine.Queries;

namespace Stock_Online.Services.Update
{

    public class StockPriceUpdateService : IStockPriceUpdateService
    {
        private readonly IStockRepository _repo;
        private readonly IHubContext<StockUpdateHub> _hub;

        public StockPriceUpdateService(IStockRepository repo, IHubContext<StockUpdateHub> hub)
        {
            _repo = repo;
            _hub = hub;
        }
        public async Task<List<StockDailyPrice>> GetDailyPricesAsync(string stockId)
        {
            if (string.IsNullOrWhiteSpace(stockId))
                throw new ArgumentException("StockId 不可為空");

            Query query = StockDailyPriceQueryBuilder.Build(stockId, null, "20000101", "20501231");
            var re = await _repo.GetPriceByQueryAsync(query);
            return re;
        }
        public async Task FetchAndSaveAsync(int year, string stockId)
        {
            if (string.IsNullOrWhiteSpace(stockId))
                throw new ArgumentException("StockId 不可為空");

            string logFile =
                $"Logs/StockUpdate_Single_{stockId}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..5]}.log";

            var jobLog = new StockUpdateJobLogger(logFile);

            jobLog.JobStart("Single Stock Update Started");
            jobLog.SingleStockInfo(stockId, year);

            var startTime = DateTime.Now;

            bool success;
            try
            {
                success = await FetchAndSaveWithRetryAsync(year, stockId, jobLog);
            }
            catch (Exception ex)
            {
                jobLog.SingleStockFail(stockId, ex);
                throw;
            }

            if (!success)
            {
                jobLog.SingleStockFail(
                    stockId,
                    new Exception("Update failed after 3 retries")
                );
                throw new Exception($"Stock {stockId} update failed");
            }

            jobLog.SingleStockSuccess(stockId);
            jobLog.JobEnd(DateTime.Now - startTime);
        }
        private async Task<bool> FetchAndSaveWithRetryAsync(
            int year,
            string stockId,
            StockUpdateJobLogger log)
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

            for (int retry = 1; retry <= 3; retry++)
            {
                try
                {
                    for (int month = 1; month <= 12; month++)
                    {
                        string date = $"{year}{month:00}01";
                        string url =
                            $"https://www.twse.com.tw/exchangeReport/STOCK_DAY" +
                            $"?response=json&date={date}&stockNo={stockId}";

                        var response =
                            await http.GetFromJsonAsync<TwseStockDayResponse>(url);

                        if (response == null || response.stat != "OK")
                            continue;

                        var models = response.data
                            .Select(row => TryMapToModel(stockId, row))
                            .Where(x => x != null)
                            .Cast<StockDailyPrice>()
                            .ToList();

                        _repo.SaveToDb(models);
                    }

                    return true; // 成功
                }
                catch (Exception ex)
                {
                    log.RetryFail(stockId, retry, ex);

                    if (retry < 3)
                        await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
            return false;
        }
        public async Task FetchAndSaveAllStockAsync(int year)
        {
            var startTime = DateTime.Now;
            var stockIds = await _repo.GetAllStockIdsAsync();
            int total = stockIds.Count;
            string logFile = $"Logs/StockUpdate_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..5]}.log";
            var jobLog = new StockUpdateJobLogger(logFile);
            jobLog.JobStart(year, total);
            int n = 1;
            int success = 0;
            int fail = 0;
            List<string> failedStocks = new List<string>();
            for (int i = 0; i < stockIds.Count; i++)
            {
                var stockId = stockIds[i];
                int current = i + 1;

                jobLog.StockStart(stockId, current, total);

                bool ok = await FetchAndSaveWithRetryAsync(year, stockId, jobLog);

                if (ok)
                {
                    success++;
                    jobLog.StockSuccess(stockId);
                    await ReportProgressAsync($" {year} 股票代號: {stockId} 已更新完成 ({current}/{total})");
                }
                else
                {
                    fail++;
                    failedStocks.Add(stockId);
                    jobLog.StockFail(stockId);
                    await ReportProgressAsync($" {year} 股票代號: {stockId} 更新失敗 ({current}/{total})");
                }
            }
            var endTime = DateTime.Now;
            jobLog.JobSummary(success, fail, total, failedStocks, startTime);
            await ReportProgressAsync($" {year} 全部股票更新完成");
        }
        private async Task ReportProgressAsync(string message)
        {
            await _hub.Clients.All.SendAsync("Progress", message);
        }
        private static DateTime ParseRocDate(string rocDate)
        {
            rocDate = rocDate.Trim();
            var p = rocDate.Split('/');
            return new DateTime(int.Parse(p[0]) + 1911, int.Parse(p[1]), int.Parse(p[2]));
        }
        private static StockDailyPrice? TryMapToModel(string stockId, List<string> row)
        {
            try
            {
                if (!TryParseDecimal(row[3], out var open)) return null;
                if (!TryParseDecimal(row[4], out var high)) return null;
                if (!TryParseDecimal(row[5], out var low)) return null;
                if (!TryParseDecimal(row[6], out var close)) return null;
                if (!TryParseDecimal(row[7], out var change)) return null;

                if (!TryParseLong(row[1], out var volume)) return null;
                if (!TryParseLong(row[2], out var amount)) return null;
                if (!TryParseInt(row[8], out var count)) return null;

                return new StockDailyPrice
                {
                    StockId = stockId,
                    TradeDate = ParseRocDate(row[0]),
                    Volume = volume,
                    Amount = amount,
                    OpenPrice = open,
                    HighPrice = high,
                    LowPrice = low,
                    ClosePrice = close,
                    PriceChange = change,
                    TradeCount = count,
                    Note = row[9]
                };
            }
            catch
            {
                // 保險用：任何非預期錯誤，直接略過
                return null;
            }
        }
        private static bool TryParseDecimal(string input, out decimal value)
        {
            input = input.Trim();

            // 常見無效值
            if (string.IsNullOrWhiteSpace(input) ||
                input == "--" ||
                input.StartsWith("X", StringComparison.OrdinalIgnoreCase))
            {
                value = 0;
                return false;
            }

            return decimal.TryParse(input, out value);
        }
        private static bool TryParseLong(string input, out long value)
        {
            input = input.Replace(",", "").Trim();
            return long.TryParse(input, out value);
        }
        private static bool TryParseInt(string input, out int value)
        {
            input = input.Replace(",", "").Trim();
            return int.TryParse(input, out value);
        }

    }
}
