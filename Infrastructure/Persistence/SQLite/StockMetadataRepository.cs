using Microsoft.Data.Sqlite;
using Stock_Online.Domain.Entities.Stock_Online.DTOs;
using Stock_Online.Domain.Interfaces.Repositories;

namespace Stock_Online.Infrastructure.Persistence.SQLite
{
    public class StockMetadataRepository : SqliteRepositoryBase, IStockMetadataRepository
    {
        public StockMetadataRepository(IConfiguration configuration) : base(configuration) { }
        public async Task<StockInfoDto> GetStockInfoAsync(string stockId)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    公司代號,
                    公司名稱,
                    公司簡稱,
                    產業別,
                    董事長,
                    總經理,
                    網址
                FROM StockList
                WHERE 公司代號 = @stockId
                LIMIT 1
            ";

            cmd.Parameters.AddWithValue("@stockId", stockId);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new StockInfoDto
            {
                StockId = reader.GetInt32(0).ToString(),
                CompanyName = reader.IsDBNull(1) ? null : reader.GetString(1),
                CompanyShortName = reader.IsDBNull(2) ? null : reader.GetString(2),
                Industry = reader.IsDBNull(3) ? null : reader.GetInt32(3).ToString(),
                Chairman = reader.IsDBNull(4) ? null : reader.GetString(4),
                GeneralManager = reader.IsDBNull(5) ? null : reader.GetString(5),
                Website = reader.IsDBNull(6) ? null : reader.GetString(6)
            };
        }
        public async Task<List<StockInfoDto>> GetStockListAsync()
        {
            var stocks = new List<StockInfoDto>();

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                    SELECT
                        公司代號,
                        公司名稱,
                        公司簡稱,
                        產業別,
                        董事長,
                        總經理,
                        網址
                    FROM StockList
                ";

            await using var reader = await cmd.ExecuteReaderAsync();

            // 使用 while 迴圈讀取所有資料列
            while (await reader.ReadAsync())
            {
                stocks.Add(new StockInfoDto
                {
                    // 注意：如果公司代號在 DB 是整數但你想轉字串，維持原邏輯使用 GetInt32().ToString()
                    StockId = reader.IsDBNull(0) ? null : reader.GetValue(0).ToString(),
                    CompanyName = reader.IsDBNull(1) ? null : reader.GetString(1),
                    CompanyShortName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Industry = reader.IsDBNull(3) ? null : reader.GetValue(3).ToString(),
                    Chairman = reader.IsDBNull(4) ? null : reader.GetString(4),
                    GeneralManager = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Website = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }

            return stocks;
        }
        public Task UpsertStockListAsync(List<StockInfoDto> stocks) => throw new NotImplementedException();
    }
}
