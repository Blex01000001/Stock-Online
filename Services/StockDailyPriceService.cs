using System.Net.Http.Json;
using Microsoft.Data.Sqlite;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;

namespace Stock_Online.Services
{

    public class StockDailyPriceService
    {
        private readonly string _dbPath;

        public StockDailyPriceService(string dbPath)
        {
            _dbPath = dbPath;
            EnsureTable();
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

                SaveToDb(models);
            }
        }

        private void EnsureTable()
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
            """
        CREATE TABLE IF NOT EXISTS StockDailyPrice (
            StockId TEXT NOT NULL,
            TradeDate TEXT NOT NULL,
            Volume INTEGER,
            Amount INTEGER,
            OpenPrice REAL,
            HighPrice REAL,
            LowPrice REAL,
            ClosePrice REAL,
            PriceChange REAL,
            TradeCount INTEGER,
            Note TEXT,
            PRIMARY KEY (StockId, TradeDate)
        );
        """;
            cmd.ExecuteNonQuery();
        }

        private void SaveToDb(List<StockDailyPrice> list)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            using var tx = conn.BeginTransaction();

            foreach (var item in list)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText =
                """
            INSERT OR REPLACE INTO StockDailyPrice
            (StockId, TradeDate, Volume, Amount, OpenPrice, HighPrice, LowPrice,
             ClosePrice, PriceChange, TradeCount, Note)
            VALUES
            (@StockId, @TradeDate, @Volume, @Amount, @Open, @High, @Low,
             @Close, @Change, @Count, @Note);
            """;

                cmd.Parameters.AddWithValue("@StockId", item.StockId);
                cmd.Parameters.AddWithValue("@TradeDate", item.TradeDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@Volume", item.Volume);
                cmd.Parameters.AddWithValue("@Amount", item.Amount);
                cmd.Parameters.AddWithValue("@Open", item.OpenPrice);
                cmd.Parameters.AddWithValue("@High", item.HighPrice);
                cmd.Parameters.AddWithValue("@Low", item.LowPrice);
                cmd.Parameters.AddWithValue("@Close", item.ClosePrice);
                cmd.Parameters.AddWithValue("@Change", item.PriceChange);
                cmd.Parameters.AddWithValue("@Count", item.TradeCount);
                cmd.Parameters.AddWithValue("@Note", item.Note);

                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        // ===== Helpers =====

        private static DateTime ParseRocDate(string rocDate)
        {
            rocDate = rocDate.Trim();
            var p = rocDate.Split('/');
            return new DateTime(int.Parse(p[0]) + 1911, int.Parse(p[1]), int.Parse(p[2]));
        }

        private static StockDailyPrice? TryMapToModel(
            string stockId,
            List<string> row)
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
        private static decimal ParsePriceChange(string value)
        {
            value = value.Trim();

            // 不比價、空值 → 視為 0
            if (string.IsNullOrWhiteSpace(value))
                return 0m;

            // X 開頭表示不比價
            if (value.StartsWith("X", StringComparison.OrdinalIgnoreCase))
                return 0m;

            return decimal.Parse(value);
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
