using System.Net.Http.Json;
using System.Security.Cryptography.Xml;
using Microsoft.Data.Sqlite;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;

namespace Stock_Online.Services.Update
{

    public class StockDailyPriceService : IStockPriceUpdateService
    {
        private readonly IStockDailyPriceRepository _repo;

        public StockDailyPriceService(IStockDailyPriceRepository repo)
        {
            _repo = repo;
        }
        public async Task<List<StockDailyPrice>> GetDailyPricesAsync(string stockId)
        {
            if (string.IsNullOrWhiteSpace(stockId))
                throw new ArgumentException("StockId 不可為空");

            var re = await _repo.GetByStockIdAsync(stockId);
            return re;
        }
        public async Task FetchAndSaveAsync(int year, string stockId)
        {
            using var http = new HttpClient();

            for (int month = 1; month <= 12; month++)
            {
                string date = $"{year}{month:00}01";
                string url =
                    $"https://www.twse.com.tw/exchangeReport/STOCK_DAY" +
                    $"?response=json&date={date}&stockNo={stockId}";

                var response = await http.GetFromJsonAsync<TwseStockDayResponse>(url);

                if (response == null || response.stat != "OK")
                    continue; // 某些月份可能還沒資料（未來月份）

                var models = response.data
                    .Select(row => TryMapToModel(stockId, row))
                    .Where(x => x != null)
                    .Cast<StockDailyPrice>()
                    .ToList();

                _repo.SaveToDb(models);
                //SaveToDb(models);
            }
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
