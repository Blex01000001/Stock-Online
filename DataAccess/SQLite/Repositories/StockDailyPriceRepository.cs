using Microsoft.Data.Sqlite;
using SqlKata;
using SqlKata.Compilers;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using System.Globalization;

namespace Stock_Online.DataAccess.SQLite.Repositories
{
    public class StockDailyPriceRepository : IStockDailyPriceRepository
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly SqliteCompiler _compiler = new();
        public StockDailyPriceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Sqlite");
            _dbPath = "stock.db";
            EnsureTable();
        }

        public async Task<List<StockDailyPrice>> GetByQueryAsync(Query query)
        {
            var result = new List<StockDailyPrice>();

            // 1️⃣ 編譯 SqlKata → SQL + Parameters
            var compiled = _compiler.Compile(query);

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = compiled.Sql;

            // 2️⃣ 參數綁定（這一步很關鍵）
            foreach (var kv in compiled.NamedBindings)
            {
                cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
            }

            // 3️⃣ Execute + Mapping
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new StockDailyPrice
                {
                    StockId = reader.GetString(reader.GetOrdinal("StockId")),

                    TradeDate = DateTime.ParseExact(
                        reader.GetString(reader.GetOrdinal("TradeDate")),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture
                    ),

                    Volume = reader.IsDBNull(reader.GetOrdinal("Volume")) ? 0 : reader.GetInt64(reader.GetOrdinal("Volume")),
                    Amount = reader.IsDBNull(reader.GetOrdinal("Amount")) ? 0 : reader.GetInt64(reader.GetOrdinal("Amount")),

                    OpenPrice = reader.IsDBNull(reader.GetOrdinal("OpenPrice")) ? 0 : reader.GetDecimal(reader.GetOrdinal("OpenPrice")),
                    HighPrice = reader.IsDBNull(reader.GetOrdinal("HighPrice")) ? 0 : reader.GetDecimal(reader.GetOrdinal("HighPrice")),
                    LowPrice = reader.IsDBNull(reader.GetOrdinal("LowPrice")) ? 0 : reader.GetDecimal(reader.GetOrdinal("LowPrice")),
                    ClosePrice = reader.IsDBNull(reader.GetOrdinal("ClosePrice")) ? 0 : reader.GetDecimal(reader.GetOrdinal("ClosePrice")),

                    PriceChange = reader.IsDBNull(reader.GetOrdinal("PriceChange")) ? 0 : reader.GetDecimal(reader.GetOrdinal("PriceChange")),
                    TradeCount = reader.IsDBNull(reader.GetOrdinal("TradeCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("TradeCount")),

                    Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? null : reader.GetString(reader.GetOrdinal("Note"))
                });
            }

            return result;
        }
        public async Task<List<StockDailyPrice>> GetByStockIdAsync(string stockId)
        {
            var list = new List<StockDailyPrice>();

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT
                StockId,
                TradeDate,
                Volume,
                Amount,
                OpenPrice,
                HighPrice,
                LowPrice,
                ClosePrice,
                PriceChange,
                TradeCount,
                Note
                FROM StockDailyPrice
                WHERE StockId = @stockId
                ORDER BY TradeDate
            ";

            cmd.Parameters.AddWithValue("@stockId", stockId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new StockDailyPrice
                {
                    StockId = reader.GetString(0),

                    // 🔴 關鍵：SQLite TEXT → DateTime
                    TradeDate = DateTime.ParseExact(
                        reader.GetString(1),
                        "yyyy-MM-dd",   // 或 yyyyMMdd，依你實際資料
                        CultureInfo.InvariantCulture
                    ),

                    Volume = reader.IsDBNull(2) ? 0 : reader.GetInt64(2),
                    Amount = reader.IsDBNull(3) ? 0 : reader.GetInt64(3),

                    OpenPrice = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                    HighPrice = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                    LowPrice = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                    ClosePrice = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),

                    PriceChange = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                    TradeCount = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),

                    Note = reader.IsDBNull(10) ? null : reader.GetString(10)
                });
            }
            return list;
        }
        public void SaveToDb(List<StockDailyPrice> list)
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


    }
}
