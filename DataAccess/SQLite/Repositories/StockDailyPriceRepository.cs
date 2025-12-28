using Microsoft.Data.Sqlite;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using System.Globalization;

namespace Stock_Online.DataAccess.SQLite.Repositories
{
    public class StockDailyPriceRepository : IStockDailyPriceRepository
    {
        private readonly string _connectionString;

        public StockDailyPriceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Sqlite");
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

        public void test(List<StockDailyPrice> stockPrices)
        {
            var re = stockPrices
                .Where(x => x.TradeDate.Year == 2025)
                .Where(x => x.TradeDate.Month == 12)
                .Where(x => x.TradeDate.Date == new DateTime(2025,12,19))
                .Select(x => new RatingModel()
                {
                    TradeDate = x.TradeDate,
                    StartDate = x.TradeDate.AddDays(-3650-180),
                    EndDate = x.TradeDate.AddDays(-3650+180),
                    NowPrice = x.ClosePrice,

                }).ToList();





        }
        
    }
}
