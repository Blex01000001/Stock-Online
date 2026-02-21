using Microsoft.Data.Sqlite;

namespace Stock_Online.DataAccess.SQLite
{
    public sealed class SqliteBatchWriter
    {
        private readonly string _connectionString;

        public SqliteBatchWriter(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 共用批次寫入：cmd/parameters 建一次，foreach 只改 Value
        /// </summary>
        public async Task<int> ExecuteAsync<T>(
            IReadOnlyList<T> items,
            string sql,
            Action<SqliteCommand> createParameters,
            Action<SqliteCommand, T> bindValues,
            CancellationToken ct = default)
        {
            if (items == null || items.Count == 0) return 0;

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var tx = await conn.BeginTransactionAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.Transaction = (SqliteTransaction)tx;
            cmd.CommandText = sql;

            // ✅ 參數只建一次
            createParameters(cmd);

            int affected = 0;
            foreach (var item in items)
            {
                bindValues(cmd, item);
                affected += await cmd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
            return affected;
        }

        public object DbValue<TVal>(TVal? value) where TVal : struct
            => value.HasValue ? value.Value : DBNull.Value;

        public object DbValue(string? value)
            => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value!;

    }
}
