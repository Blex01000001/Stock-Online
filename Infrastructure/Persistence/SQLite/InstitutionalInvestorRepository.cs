using Microsoft.Data.Sqlite;
using SqlKata;
using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Interfaces.Repositories;
using System.Collections.Generic;

namespace Stock_Online.Infrastructure.Persistence.SQLite
{
    public class InstitutionalInvestorRepository : SqliteRepositoryBase, IInstitutionalInvestorRepository
    {
        public InstitutionalInvestorRepository(IConfiguration configuration) : base(configuration)
        {
            EnsureInstitutionalInvestorsBuySellTable();
            EnsureShareHoldingTable();
        }
        public Task<List<StockInstitutionalInvestorsBuySell>> GetDailyTradesAsync(Query query)
        {
            return QueryAsync(query, reader => new StockInstitutionalInvestorsBuySell
            {
                StockId = reader.GetString(reader.GetOrdinal("StockId")),
                Date = reader.GetString(reader.GetOrdinal("Date")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Buy = Convert.ToInt64(reader.GetValue(reader.GetOrdinal("Buy"))),
                Sell = Convert.ToInt64(reader.GetValue(reader.GetOrdinal("Sell"))),
            });
        }
        public async Task UpsertDailyTradesAsync(List<StockInstitutionalInvestorsBuySell> trades)
        {
            const string sql = @"
                INSERT INTO StockInstitutionalInvestorsBuySell
                (
                    StockId, Date, Name, Buy, Sell
                )
                VALUES
                (
                    @StockId, @Date, @Name, @Buy, @Sell
                )
                ON CONFLICT(StockId, Date, Name) DO UPDATE SET
                    Buy = excluded.Buy,
                    Sell = excluded.Sell;
            "
            ;

            await _writer.ExecuteAsync(
                trades,
                sql,
                createParameters: cmd =>
                {
                    cmd.Parameters.Add(new SqliteParameter("@StockId", ""));
                    cmd.Parameters.Add(new SqliteParameter("@Date", ""));
                    cmd.Parameters.Add(new SqliteParameter("@Name", ""));
                    cmd.Parameters.Add(new SqliteParameter("@Buy", 0L));
                    cmd.Parameters.Add(new SqliteParameter("@Sell", 0L));
                },
                bindValues: (cmd, x) =>
                {
                    cmd.Parameters["@StockId"].Value = x.StockId;
                    cmd.Parameters["@Date"].Value = x.Date;
                    cmd.Parameters["@Name"].Value = x.Name;
                    cmd.Parameters["@Buy"].Value = x.Buy;
                    cmd.Parameters["@Sell"].Value = x.Sell;
                });

        }
        public Task<List<StockShareholding>> GetShareHoldingsAsync(Query query)
        {
            return QueryAsync(query, reader => new StockShareholding
            {
                StockId = reader.GetString(reader.GetOrdinal("StockId")),
                Date = reader.GetString(reader.GetOrdinal("Date")),

                StockName = reader.IsDBNull(reader.GetOrdinal("StockName")) ? null : reader.GetString(reader.GetOrdinal("StockName")),
                InternationalCode = reader.IsDBNull(reader.GetOrdinal("InternationalCode")) ? null : reader.GetString(reader.GetOrdinal("InternationalCode")),

                ForeignInvestmentRemainingShares = reader.IsDBNull(reader.GetOrdinal("ForeignInvestmentRemainingShares")) ? null : Convert.ToInt64(reader.GetValue(reader.GetOrdinal("ForeignInvestmentRemainingShares"))),
                ForeignInvestmentShares = reader.IsDBNull(reader.GetOrdinal("ForeignInvestmentShares")) ? null : Convert.ToInt64(reader.GetValue(reader.GetOrdinal("ForeignInvestmentShares"))),

                ForeignInvestmentRemainRatio = reader.IsDBNull(reader.GetOrdinal("ForeignInvestmentRemainRatio")) ? null : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("ForeignInvestmentRemainRatio"))),
                ForeignInvestmentSharesRatio = reader.IsDBNull(reader.GetOrdinal("ForeignInvestmentSharesRatio")) ? null : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("ForeignInvestmentSharesRatio"))),

                ForeignInvestmentUpperLimitRatio = reader.IsDBNull(reader.GetOrdinal("ForeignInvestmentUpperLimitRatio")) ? null : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("ForeignInvestmentUpperLimitRatio"))),
                ChineseInvestmentUpperLimitRatio = reader.IsDBNull(reader.GetOrdinal("ChineseInvestmentUpperLimitRatio")) ? null : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("ChineseInvestmentUpperLimitRatio"))),

                NumberOfSharesIssued = reader.IsDBNull(reader.GetOrdinal("NumberOfSharesIssued")) ? null : Convert.ToInt64(reader.GetValue(reader.GetOrdinal("NumberOfSharesIssued"))),

                RecentlyDeclareDate = reader.IsDBNull(reader.GetOrdinal("RecentlyDeclareDate")) ? null : reader.GetString(reader.GetOrdinal("RecentlyDeclareDate")),
                Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? null : reader.GetString(reader.GetOrdinal("Note")),
            });

        }
        public async Task UpsertGetShareHoldingsAsync(List<StockShareholding> ownerships)
        {
            const string sql = @"
                INSERT INTO StockShareholding
                (
                    StockId, Date, StockName, InternationalCode,
                    ForeignInvestmentRemainingShares, ForeignInvestmentShares,
                    ForeignInvestmentRemainRatio, ForeignInvestmentSharesRatio,
                    ForeignInvestmentUpperLimitRatio, ChineseInvestmentUpperLimitRatio,
                    NumberOfSharesIssued, RecentlyDeclareDate, Note
                )
                VALUES
                (
                    @StockId, @Date, @StockName, @InternationalCode,
                    @ForeignInvestmentRemainingShares, @ForeignInvestmentShares,
                    @ForeignInvestmentRemainRatio, @ForeignInvestmentSharesRatio,
                    @ForeignInvestmentUpperLimitRatio, @ChineseInvestmentUpperLimitRatio,
                    @NumberOfSharesIssued, @RecentlyDeclareDate, @Note
                )
                ON CONFLICT(StockId, Date) DO UPDATE SET
                    StockName = excluded.StockName,
                    InternationalCode = excluded.InternationalCode,
                    ForeignInvestmentRemainingShares = excluded.ForeignInvestmentRemainingShares,
                    ForeignInvestmentShares = excluded.ForeignInvestmentShares,
                    ForeignInvestmentRemainRatio = excluded.ForeignInvestmentRemainRatio,
                    ForeignInvestmentSharesRatio = excluded.ForeignInvestmentSharesRatio,
                    ForeignInvestmentUpperLimitRatio = excluded.ForeignInvestmentUpperLimitRatio,
                    ChineseInvestmentUpperLimitRatio = excluded.ChineseInvestmentUpperLimitRatio,
                    NumberOfSharesIssued = excluded.NumberOfSharesIssued,
                    RecentlyDeclareDate = excluded.RecentlyDeclareDate,
                    Note = excluded.Note;
            ";

            await _writer.ExecuteAsync(
                ownerships,
                sql,
                createParameters: cmd =>
                {
                    cmd.Parameters.Add(new SqliteParameter("@StockId", ""));
                    cmd.Parameters.Add(new SqliteParameter("@Date", ""));
                    cmd.Parameters.Add(new SqliteParameter("@StockName", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@InternationalCode", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@ForeignInvestmentRemainingShares", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@ForeignInvestmentShares", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@ForeignInvestmentRemainRatio", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@ForeignInvestmentSharesRatio", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@ForeignInvestmentUpperLimitRatio", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@ChineseInvestmentUpperLimitRatio", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@NumberOfSharesIssued", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@RecentlyDeclareDate", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@Note", DBNull.Value));
                },
                bindValues: (cmd, x) =>
                {
                    cmd.Parameters["@StockId"].Value = x.StockId;
                    cmd.Parameters["@Date"].Value = x.Date;
                    cmd.Parameters["@StockName"].Value = _writer.DbValue(x.StockName);
                    cmd.Parameters["@InternationalCode"].Value = _writer.DbValue(x.InternationalCode);

                    cmd.Parameters["@ForeignInvestmentRemainingShares"].Value = (object?)x.ForeignInvestmentRemainingShares ?? DBNull.Value;
                    cmd.Parameters["@ForeignInvestmentShares"].Value = (object?)x.ForeignInvestmentShares ?? DBNull.Value;
                    cmd.Parameters["@ForeignInvestmentRemainRatio"].Value = (object?)x.ForeignInvestmentRemainRatio ?? DBNull.Value;
                    cmd.Parameters["@ForeignInvestmentSharesRatio"].Value = (object?)x.ForeignInvestmentSharesRatio ?? DBNull.Value;
                    cmd.Parameters["@ForeignInvestmentUpperLimitRatio"].Value = (object?)x.ForeignInvestmentUpperLimitRatio ?? DBNull.Value;
                    cmd.Parameters["@ChineseInvestmentUpperLimitRatio"].Value = (object?)x.ChineseInvestmentUpperLimitRatio ?? DBNull.Value;

                    cmd.Parameters["@NumberOfSharesIssued"].Value = (object?)x.NumberOfSharesIssued ?? DBNull.Value;
                    cmd.Parameters["@RecentlyDeclareDate"].Value = _writer.DbValue(x.RecentlyDeclareDate);
                    cmd.Parameters["@Note"].Value = _writer.DbValue(x.Note);
                });

        }
        private void EnsureInstitutionalInvestorsBuySellTable()
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS StockInstitutionalInvestorsBuySell (
                StockId TEXT NOT NULL,
                Date TEXT NOT NULL,
                Name TEXT NOT NULL,
                Buy INTEGER NOT NULL,
                Sell INTEGER NOT NULL,
                PRIMARY KEY (StockId, Date, Name)
            );
            """;

            cmd.ExecuteNonQuery();

            // ✅ 常用查詢：StockId + Date
            using var idx = conn.CreateCommand();
            idx.CommandText =
            """
                CREATE INDEX IF NOT EXISTS IDX_StockInstitutionalInvestorsBuySell_StockId_Date
                ON StockInstitutionalInvestorsBuySell (StockId, Date);
            """;
            idx.ExecuteNonQuery();
        }
        private void EnsureShareHoldingTable()
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS StockShareholding (
                StockId TEXT NOT NULL,
                Date TEXT NOT NULL,

                StockName TEXT,
                InternationalCode TEXT,

                ForeignInvestmentRemainingShares INTEGER,
                ForeignInvestmentShares INTEGER,
                ForeignInvestmentRemainRatio REAL,
                ForeignInvestmentSharesRatio REAL,
                ForeignInvestmentUpperLimitRatio REAL,
                ChineseInvestmentUpperLimitRatio REAL,
                NumberOfSharesIssued INTEGER,

                RecentlyDeclareDate TEXT,
                Note TEXT,

                PRIMARY KEY (StockId, Date)
            );
            """;

            cmd.ExecuteNonQuery();
        }
    }
}
