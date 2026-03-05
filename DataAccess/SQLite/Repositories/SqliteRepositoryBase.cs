using Microsoft.Data.Sqlite;
using SqlKata.Compilers;
using SqlKata;

namespace Stock_Online.DataAccess.SQLite.Repositories
{
    public abstract class SqliteRepositoryBase
    {
        protected readonly string _connectionString;
        protected readonly string _dbPath;
        protected readonly SqliteBatchWriter _writer;
        protected readonly SqliteCompiler _compiler = new();

        // 讓子類別透過建構函式傳遞 Configuration
        protected SqliteRepositoryBase(IConfiguration configuration)
        {
            _dbPath = "stock.db";
            _connectionString = configuration.GetConnectionString("Sqlite")
                ?? throw new InvalidOperationException("Connection string not found.");

            // 這裡可以考慮將 Writer 也做成單例注入，但目前先保持你的邏輯
            _writer = new SqliteBatchWriter(_connectionString);
        }

        // 共通的查詢邏輯，設為 protected 讓子類別呼叫
        protected async Task<List<T>> QueryAsync<T>(Query query, Func<SqliteDataReader, T> map)
        {
            var result = new List<T>();
            var compiled = _compiler.Compile(query);

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = compiled.Sql;

            foreach (var kv in compiled.NamedBindings)
                cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(map(reader));

            return result;
        }
    }
}
