using Microsoft.Data.Sqlite;
using SqlKata;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Enums;
using System.Collections.Generic;
using System.Globalization;

namespace Stock_Online.DataAccess.SQLite.Repositories
{
    public class TradeDataRepository : SqliteRepositoryBase, ITradeDataRepository
    {
        public TradeDataRepository(IConfiguration configuration) : base(configuration)
        {
            EnsurePriceTable();
            EnsureDividendTable();
        }
        public Task<List<StockDailyPrice>> GetDailyPricesAsync(Query query)
        {
            return QueryAsync(query, reader => new StockDailyPrice
            {
                StockId = reader.GetString(reader.GetOrdinal("StockId")),
                TradeDate = DateTime.ParseExact(
                    reader.GetString(reader.GetOrdinal("TradeDate")),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture),

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
        public async Task UpsertDailyPricesAsync(List<StockDailyPrice> prices)
        {
            const string sql = """
                INSERT OR REPLACE INTO StockDailyPrice
                (StockId, TradeDate, Volume, Amount, OpenPrice, HighPrice, LowPrice,
                 ClosePrice, PriceChange, TradeCount, Note)
                VALUES
                (@StockId, @TradeDate, @Volume, @Amount, @Open, @High, @Low,
                 @Close, @Change, @Count, @Note);
                """
            ;

            await _writer.ExecuteAsync(
                prices,
                sql,
                createParameters: cmd =>
                {
                    cmd.Parameters.Add(new SqliteParameter("@StockId", ""));
                    cmd.Parameters.Add(new SqliteParameter("@TradeDate", ""));
                    cmd.Parameters.Add(new SqliteParameter("@Volume", 0L));
                    cmd.Parameters.Add(new SqliteParameter("@Amount", 0L));
                    cmd.Parameters.Add(new SqliteParameter("@Open", 0m));
                    cmd.Parameters.Add(new SqliteParameter("@High", 0m));
                    cmd.Parameters.Add(new SqliteParameter("@Low", 0m));
                    cmd.Parameters.Add(new SqliteParameter("@Close", 0m));
                    cmd.Parameters.Add(new SqliteParameter("@Change", 0m));
                    cmd.Parameters.Add(new SqliteParameter("@Count", 0));
                    cmd.Parameters.Add(new SqliteParameter("@Note", ""));
                },
                bindValues: (cmd, x) =>
                {
                    cmd.Parameters["@StockId"].Value = x.StockId;
                    cmd.Parameters["@TradeDate"].Value = x.TradeDate.ToString("yyyy-MM-dd");
                    cmd.Parameters["@Volume"].Value = x.Volume;
                    cmd.Parameters["@Amount"].Value = x.Amount;
                    cmd.Parameters["@Open"].Value = x.OpenPrice;
                    cmd.Parameters["@High"].Value = x.HighPrice;
                    cmd.Parameters["@Low"].Value = x.LowPrice;
                    cmd.Parameters["@Close"].Value = x.ClosePrice;
                    cmd.Parameters["@Change"].Value = x.PriceChange;
                    cmd.Parameters["@Count"].Value = x.TradeCount;
                    cmd.Parameters["@Note"].Value = (object?)x.Note ?? DBNull.Value;
                });
        }
        public Task<List<StockDividend>> GetDividendsAsync(Query query)
        {
            return QueryAsync(query, reader => new StockDividend
            {
                StockId = reader.GetString(reader.GetOrdinal("StockId")),
                Date = DateTime.ParseExact(
                    reader.GetString(reader.GetOrdinal("Date")),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture),
                Year = reader.GetString(reader.GetOrdinal("Year")),

                StockEarningsDistribution = reader.IsDBNull(reader.GetOrdinal("StockEarningsDistribution")) ? null : reader.GetDecimal(reader.GetOrdinal("StockEarningsDistribution")),
                StockStatutorySurplus = reader.IsDBNull(reader.GetOrdinal("StockStatutorySurplus")) ? null : reader.GetDecimal(reader.GetOrdinal("StockStatutorySurplus")),
                StockExDividendTradingDate = reader.IsDBNull(reader.GetOrdinal("StockExDividendTradingDate")) ? null : reader.GetString(reader.GetOrdinal("StockExDividendTradingDate")),

                TotalEmployeeStockDividend = reader.IsDBNull(reader.GetOrdinal("TotalEmployeeStockDividend")) ? null : reader.GetDecimal(reader.GetOrdinal("TotalEmployeeStockDividend")),
                TotalEmployeeStockDividendAmount = reader.IsDBNull(reader.GetOrdinal("TotalEmployeeStockDividendAmount")) ? null : reader.GetDecimal(reader.GetOrdinal("TotalEmployeeStockDividendAmount")),
                RatioOfEmployeeStockDividendOfTotal = reader.IsDBNull(reader.GetOrdinal("RatioOfEmployeeStockDividendOfTotal")) ? null : reader.GetDecimal(reader.GetOrdinal("RatioOfEmployeeStockDividendOfTotal")),
                RatioOfEmployeeStockDividend = reader.IsDBNull(reader.GetOrdinal("RatioOfEmployeeStockDividend")) ? null : reader.GetDecimal(reader.GetOrdinal("RatioOfEmployeeStockDividend")),

                CashEarningsDistribution = reader.IsDBNull(reader.GetOrdinal("CashEarningsDistribution")) ? null : reader.GetDecimal(reader.GetOrdinal("CashEarningsDistribution")),
                CashStatutorySurplus = reader.IsDBNull(reader.GetOrdinal("CashStatutorySurplus")) ? null : reader.GetDecimal(reader.GetOrdinal("CashStatutorySurplus")),
                CashExDividendTradingDate = reader.IsDBNull(reader.GetOrdinal("CashExDividendTradingDate")) ? null : reader.GetString(reader.GetOrdinal("CashExDividendTradingDate")),
                CashDividendPaymentDate = reader.IsDBNull(reader.GetOrdinal("CashDividendPaymentDate")) ? null : reader.GetString(reader.GetOrdinal("CashDividendPaymentDate")),

                TotalEmployeeCashDividend = reader.IsDBNull(reader.GetOrdinal("TotalEmployeeCashDividend"))
                    ? null
                    : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("TotalEmployeeCashDividend"))),

                TotalNumberOfCashCapitalIncrease = reader.IsDBNull(reader.GetOrdinal("TotalNumberOfCashCapitalIncrease")) ? null : reader.GetDecimal(reader.GetOrdinal("TotalNumberOfCashCapitalIncrease")),
                CashIncreaseSubscriptionRate = reader.IsDBNull(reader.GetOrdinal("CashIncreaseSubscriptionRate")) ? null : reader.GetDecimal(reader.GetOrdinal("CashIncreaseSubscriptionRate")),
                CashIncreaseSubscriptionPrice = reader.IsDBNull(reader.GetOrdinal("CashIncreaseSubscriptionPrice")) ? null : reader.GetDecimal(reader.GetOrdinal("CashIncreaseSubscriptionPrice")),

                RemunerationOfDirectorsAndSupervisors = reader.IsDBNull(reader.GetOrdinal("RemunerationOfDirectorsAndSupervisors"))
                    ? null
                    : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("RemunerationOfDirectorsAndSupervisors"))),

                ParticipateDistributionOfTotalShares = reader.IsDBNull(reader.GetOrdinal("ParticipateDistributionOfTotalShares")) ? null : reader.GetDecimal(reader.GetOrdinal("ParticipateDistributionOfTotalShares")),

                AnnouncementDate = reader.IsDBNull(reader.GetOrdinal("AnnouncementDate")) ? null : reader.GetString(reader.GetOrdinal("AnnouncementDate")),
                AnnouncementTime = reader.IsDBNull(reader.GetOrdinal("AnnouncementTime")) ? null : reader.GetString(reader.GetOrdinal("AnnouncementTime")),
            });

        }
        public async  Task UpsertDividendsAsync(List<StockDividend> dividends)
        {
            const string sql = """
            INSERT OR REPLACE INTO StockDividend
            (
                StockId, Date, Year,
                StockEarningsDistribution, StockStatutorySurplus, StockExDividendTradingDate,
                TotalEmployeeStockDividend, TotalEmployeeStockDividendAmount,
                RatioOfEmployeeStockDividendOfTotal, RatioOfEmployeeStockDividend,
                CashEarningsDistribution, CashStatutorySurplus,
                CashExDividendTradingDate, CashDividendPaymentDate,
                TotalEmployeeCashDividend, TotalNumberOfCashCapitalIncrease,
                CashIncreaseSubscriptionRate, CashIncreaseSubscriptionPrice,
                RemunerationOfDirectorsAndSupervisors,
                ParticipateDistributionOfTotalShares,
                AnnouncementDate, AnnouncementTime
            )
            VALUES
            (
                @StockId, @Date, @Year,
                @StockEarningsDistribution, @StockStatutorySurplus, @StockExDividendTradingDate,
                @TotalEmployeeStockDividend, @TotalEmployeeStockDividendAmount,
                @RatioOfEmployeeStockDividendOfTotal, @RatioOfEmployeeStockDividend,
                @CashEarningsDistribution, @CashStatutorySurplus,
                @CashExDividendTradingDate, @CashDividendPaymentDate,
                @TotalEmployeeCashDividend, @TotalNumberOfCashCapitalIncrease,
                @CashIncreaseSubscriptionRate, @CashIncreaseSubscriptionPrice,
                @RemunerationOfDirectorsAndSupervisors,
                @ParticipateDistributionOfTotalShares,
                @AnnouncementDate, @AnnouncementTime
            );
            """;

            await _writer.ExecuteAsync(
                dividends,
                sql,
                createParameters: cmd =>
                {
                    cmd.Parameters.Add(new SqliteParameter("@StockId", ""));
                    cmd.Parameters.Add(new SqliteParameter("@Date", ""));
                    cmd.Parameters.Add(new SqliteParameter("@Year", ""));

                    cmd.Parameters.Add(new SqliteParameter("@StockEarningsDistribution", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@StockStatutorySurplus", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@StockExDividendTradingDate", DBNull.Value));

                    cmd.Parameters.Add(new SqliteParameter("@TotalEmployeeStockDividend", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@TotalEmployeeStockDividendAmount", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@RatioOfEmployeeStockDividendOfTotal", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@RatioOfEmployeeStockDividend", DBNull.Value));

                    cmd.Parameters.Add(new SqliteParameter("@CashEarningsDistribution", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@CashStatutorySurplus", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@CashExDividendTradingDate", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@CashDividendPaymentDate", DBNull.Value));

                    cmd.Parameters.Add(new SqliteParameter("@TotalEmployeeCashDividend", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@TotalNumberOfCashCapitalIncrease", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@CashIncreaseSubscriptionRate", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@CashIncreaseSubscriptionPrice", DBNull.Value));

                    cmd.Parameters.Add(new SqliteParameter("@RemunerationOfDirectorsAndSupervisors", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@ParticipateDistributionOfTotalShares", DBNull.Value));

                    cmd.Parameters.Add(new SqliteParameter("@AnnouncementDate", DBNull.Value));
                    cmd.Parameters.Add(new SqliteParameter("@AnnouncementTime", DBNull.Value));
                },
                bindValues: (cmd, x) =>
                {
                    cmd.Parameters["@StockId"].Value = x.StockId;
                    cmd.Parameters["@Date"].Value = x.Date.ToString("yyyy-MM-dd");
                    cmd.Parameters["@Year"].Value = _writer.DbValue(x.Year);

                    cmd.Parameters["@StockEarningsDistribution"].Value = _writer.DbValue(x.StockEarningsDistribution);
                    cmd.Parameters["@StockStatutorySurplus"].Value = _writer.DbValue(x.StockStatutorySurplus);
                    cmd.Parameters["@StockExDividendTradingDate"].Value = _writer.DbValue(x.StockExDividendTradingDate);

                    cmd.Parameters["@TotalEmployeeStockDividend"].Value = _writer.DbValue(x.TotalEmployeeStockDividend);
                    cmd.Parameters["@TotalEmployeeStockDividendAmount"].Value = _writer.DbValue(x.TotalEmployeeStockDividendAmount);
                    cmd.Parameters["@RatioOfEmployeeStockDividendOfTotal"].Value = _writer.DbValue(x.RatioOfEmployeeStockDividendOfTotal);
                    cmd.Parameters["@RatioOfEmployeeStockDividend"].Value = _writer.DbValue(x.RatioOfEmployeeStockDividend);

                    cmd.Parameters["@CashEarningsDistribution"].Value = _writer.DbValue(x.CashEarningsDistribution);
                    cmd.Parameters["@CashStatutorySurplus"].Value = _writer.DbValue(x.CashStatutorySurplus);
                    cmd.Parameters["@CashExDividendTradingDate"].Value = _writer.DbValue(x.CashExDividendTradingDate);
                    cmd.Parameters["@CashDividendPaymentDate"].Value = _writer.DbValue(x.CashDividendPaymentDate);

                    cmd.Parameters["@TotalEmployeeCashDividend"].Value = _writer.DbValue(x.TotalEmployeeCashDividend);
                    cmd.Parameters["@TotalNumberOfCashCapitalIncrease"].Value = _writer.DbValue(x.TotalNumberOfCashCapitalIncrease);
                    cmd.Parameters["@CashIncreaseSubscriptionRate"].Value = _writer.DbValue(x.CashIncreaseSubscriptionRate);
                    cmd.Parameters["@CashIncreaseSubscriptionPrice"].Value = _writer.DbValue(x.CashIncreaseSubscriptionPrice);

                    cmd.Parameters["@RemunerationOfDirectorsAndSupervisors"].Value = _writer.DbValue(x.RemunerationOfDirectorsAndSupervisors);
                    cmd.Parameters["@ParticipateDistributionOfTotalShares"].Value = _writer.DbValue(x.ParticipateDistributionOfTotalShares);

                    cmd.Parameters["@AnnouncementDate"].Value = _writer.DbValue(x.AnnouncementDate);
                    cmd.Parameters["@AnnouncementTime"].Value = _writer.DbValue(x.AnnouncementTime);
                });
        }
        public Task<List<StockCorporateAction>> GetCorporateActionsAsync(Query query)
        {
            return QueryAsync(query, reader => new StockCorporateAction
            {
                StockId = reader.GetString(reader.GetOrdinal("StockId")),
                ActionType = Enum.Parse<CorporateActionType>(
                    reader.GetString(reader.GetOrdinal("ActionType"))
                ),
                ExDate = DateTime.Parse(
                    reader.GetString(reader.GetOrdinal("ExDate"))
                ),
                Ratio = reader.IsDBNull(reader.GetOrdinal("Ratio"))
                    ? null
                    : reader.GetDecimal(reader.GetOrdinal("Ratio")),
                CashAmount = reader.IsDBNull(reader.GetOrdinal("CashAmount"))
                    ? null
                    : reader.GetDecimal(reader.GetOrdinal("CashAmount")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Description"))
            });

        }
        public async  Task UpsertCorporateActionsAsync(List<StockCorporateAction> actions) => throw new NotImplementedException();
        private void EnsurePriceTable()
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
        private void EnsureDividendTable()
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS StockDividend (
                StockId TEXT NOT NULL,
                Date TEXT NOT NULL,
                Year TEXT NOT NULL,

                StockEarningsDistribution REAL,
                StockStatutorySurplus REAL,
                StockExDividendTradingDate TEXT,

                TotalEmployeeStockDividend REAL,
                TotalEmployeeStockDividendAmount REAL,
                RatioOfEmployeeStockDividendOfTotal REAL,
                RatioOfEmployeeStockDividend REAL,

                CashEarningsDistribution REAL,
                CashStatutorySurplus REAL,
                CashExDividendTradingDate TEXT,
                CashDividendPaymentDate TEXT,

                TotalEmployeeCashDividend INTEGER,
                TotalNumberOfCashCapitalIncrease REAL,
                CashIncreaseSubscriptionRate REAL,
                CashIncreaseSubscriptionPrice REAL,

                RemunerationOfDirectorsAndSupervisors INTEGER,
                ParticipateDistributionOfTotalShares REAL,

                AnnouncementDate TEXT,
                AnnouncementTime TEXT,

                PRIMARY KEY (StockId, Date)
            );
            """;

            cmd.ExecuteNonQuery();
        }

    }
}
