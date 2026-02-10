using Microsoft.Data.Sqlite;
using SqlKata;
using SqlKata.Compilers;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Entities.Stock_Online.DTOs;
using Stock_Online.Domain.Enums;
using Stock_Online.DTOs;
using System.Globalization;

namespace Stock_Online.DataAccess.SQLite.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly SqliteCompiler _compiler = new();
        public StockRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Sqlite");
            _dbPath = "stock.db";
            EnsureTable();
            EnsureDividendTable();
            EnsureShareholdingTable();
        }
        public async Task<List<StockDailyPrice>> GetPriceByQueryAsync(Query query)
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
        public async Task<List<StockDividend>> GetDividendByQueryAsync(Query query)
        {
            var result = new List<StockDividend>();

            // 1️⃣ 編譯 SqlKata → SQL + Parameters
            var compiled = _compiler.Compile(query);

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = compiled.Sql;

            // 2️⃣ 參數綁定
            foreach (var kv in compiled.NamedBindings)
            {
                cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
            }

            // 3️⃣ Execute + Mapping
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new StockDividend
                {
                    StockId = reader.GetString(reader.GetOrdinal("StockId")),

                    Date = DateTime.ParseExact(
                        reader.GetString(reader.GetOrdinal("Date")),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture
                    ),

                    Year = reader.GetString(reader.GetOrdinal("Year")),

                    StockEarningsDistribution =
                        reader.IsDBNull(reader.GetOrdinal("StockEarningsDistribution"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("StockEarningsDistribution")),

                    StockStatutorySurplus =
                        reader.IsDBNull(reader.GetOrdinal("StockStatutorySurplus"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("StockStatutorySurplus")),

                    StockExDividendTradingDate =
                        reader.IsDBNull(reader.GetOrdinal("StockExDividendTradingDate"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("StockExDividendTradingDate")),

                    TotalEmployeeStockDividend =
                        reader.IsDBNull(reader.GetOrdinal("TotalEmployeeStockDividend"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("TotalEmployeeStockDividend")),

                    TotalEmployeeStockDividendAmount =
                        reader.IsDBNull(reader.GetOrdinal("TotalEmployeeStockDividendAmount"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("TotalEmployeeStockDividendAmount")),

                    RatioOfEmployeeStockDividendOfTotal =
                        reader.IsDBNull(reader.GetOrdinal("RatioOfEmployeeStockDividendOfTotal"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("RatioOfEmployeeStockDividendOfTotal")),

                    RatioOfEmployeeStockDividend =
                        reader.IsDBNull(reader.GetOrdinal("RatioOfEmployeeStockDividend"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("RatioOfEmployeeStockDividend")),

                    CashEarningsDistribution =
                        reader.IsDBNull(reader.GetOrdinal("CashEarningsDistribution"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("CashEarningsDistribution")),

                    CashStatutorySurplus =
                        reader.IsDBNull(reader.GetOrdinal("CashStatutorySurplus"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("CashStatutorySurplus")),

                    CashExDividendTradingDate =
                        reader.IsDBNull(reader.GetOrdinal("CashExDividendTradingDate"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("CashExDividendTradingDate")),

                    CashDividendPaymentDate =
                        reader.IsDBNull(reader.GetOrdinal("CashDividendPaymentDate"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("CashDividendPaymentDate")),

                    TotalEmployeeCashDividend =
                        reader.IsDBNull(reader.GetOrdinal("TotalEmployeeCashDividend"))
                            ? null
                            : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("TotalEmployeeCashDividend"))),

                    TotalNumberOfCashCapitalIncrease =
                        reader.IsDBNull(reader.GetOrdinal("TotalNumberOfCashCapitalIncrease"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("TotalNumberOfCashCapitalIncrease")),

                    CashIncreaseSubscriptionRate =
                        reader.IsDBNull(reader.GetOrdinal("CashIncreaseSubscriptionRate"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("CashIncreaseSubscriptionRate")),

                    CashIncreaseSubscriptionPrice =
                        reader.IsDBNull(reader.GetOrdinal("CashIncreaseSubscriptionPrice"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("CashIncreaseSubscriptionPrice")),

                    RemunerationOfDirectorsAndSupervisors =
                        reader.IsDBNull(reader.GetOrdinal("RemunerationOfDirectorsAndSupervisors"))
                            ? null
                            : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("RemunerationOfDirectorsAndSupervisors"))),

                    ParticipateDistributionOfTotalShares =
                        reader.IsDBNull(reader.GetOrdinal("ParticipateDistributionOfTotalShares"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("ParticipateDistributionOfTotalShares")),

                    AnnouncementDate =
                        reader.IsDBNull(reader.GetOrdinal("AnnouncementDate"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("AnnouncementDate")),

                    AnnouncementTime =
                        reader.IsDBNull(reader.GetOrdinal("AnnouncementTime"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("AnnouncementTime"))
                });
            }

            return result;
        }
        public async Task<List<StockCorporateAction>> GetCorporateActionsAsync(string stockId)
        {
            var result = new List<StockCorporateAction>();

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    StockId,
                    ActionType,
                    ExDate,
                    Ratio,
                    CashAmount,
                    Description
                FROM StockCorporateAction
                WHERE StockId = @stockId
                ORDER BY ExDate DESC
            ";

            cmd.Parameters.AddWithValue("@stockId", stockId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new StockCorporateAction
                {
                    StockId = reader.GetString(0),
                    ActionType = Enum.Parse<CorporateActionType>(reader.GetString(1)),
                    ExDate = DateTime.Parse(reader.GetString(2)),
                    Ratio = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    CashAmount = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    Description = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }

            return result;
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
        public void SaveDividendToDb(List<StockDividend> list)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();

            using var tx = conn.BeginTransaction();

            foreach (var item in list)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText =
                """
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

                cmd.Parameters.AddWithValue("@StockId", item.StockId);
                cmd.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@Year", DbValue(item.Year));

                cmd.Parameters.AddWithValue("@StockEarningsDistribution", DbValue(item.StockEarningsDistribution));
                cmd.Parameters.AddWithValue("@StockStatutorySurplus", DbValue(item.StockStatutorySurplus));
                cmd.Parameters.AddWithValue("@StockExDividendTradingDate", DbValue(item.StockExDividendTradingDate));

                cmd.Parameters.AddWithValue("@TotalEmployeeStockDividend", DbValue(item.TotalEmployeeStockDividend));
                cmd.Parameters.AddWithValue("@TotalEmployeeStockDividendAmount", DbValue(item.TotalEmployeeStockDividendAmount));
                cmd.Parameters.AddWithValue("@RatioOfEmployeeStockDividendOfTotal", DbValue(item.RatioOfEmployeeStockDividendOfTotal));
                cmd.Parameters.AddWithValue("@RatioOfEmployeeStockDividend", DbValue(item.RatioOfEmployeeStockDividend));

                cmd.Parameters.AddWithValue("@CashEarningsDistribution", DbValue(item.CashEarningsDistribution));
                cmd.Parameters.AddWithValue("@CashStatutorySurplus", DbValue(item.CashStatutorySurplus));
                cmd.Parameters.AddWithValue("@CashExDividendTradingDate", DbValue(item.CashExDividendTradingDate));
                cmd.Parameters.AddWithValue("@CashDividendPaymentDate", DbValue(item.CashDividendPaymentDate));

                cmd.Parameters.AddWithValue("@TotalEmployeeCashDividend", DbValue(item.TotalEmployeeCashDividend));
                cmd.Parameters.AddWithValue("@TotalNumberOfCashCapitalIncrease", DbValue(item.TotalNumberOfCashCapitalIncrease));
                cmd.Parameters.AddWithValue("@CashIncreaseSubscriptionRate", DbValue(item.CashIncreaseSubscriptionRate));
                cmd.Parameters.AddWithValue("@CashIncreaseSubscriptionPrice",   DbValue(item.CashIncreaseSubscriptionPrice));

                cmd.Parameters.AddWithValue("@RemunerationOfDirectorsAndSupervisors", DbValue(item.RemunerationOfDirectorsAndSupervisors));
                cmd.Parameters.AddWithValue("@ParticipateDistributionOfTotalShares", DbValue(item.ParticipateDistributionOfTotalShares));

                cmd.Parameters.AddWithValue("@AnnouncementDate", DbValue(item.AnnouncementDate));
                cmd.Parameters.AddWithValue("@AnnouncementTime", DbValue(item.AnnouncementTime));

                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }
        public async Task SaveShareholdingToDb(List<StockShareholding> list)
        {
            if (list == null || list.Count == 0) return;

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            await using var tx = await conn.BeginTransactionAsync();

            await using var cmd = conn.CreateCommand();
            cmd.Transaction = (SqliteTransaction?)tx;

            cmd.CommandText = @"
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

            // 先建參數（避免迴圈一直 Add）
            var pStockId = cmd.CreateParameter(); pStockId.ParameterName = "@StockId"; cmd.Parameters.Add(pStockId);
            var pDate = cmd.CreateParameter(); pDate.ParameterName = "@Date"; cmd.Parameters.Add(pDate);
            var pStockName = cmd.CreateParameter(); pStockName.ParameterName = "@StockName"; cmd.Parameters.Add(pStockName);
            var pInternationalCode = cmd.CreateParameter(); pInternationalCode.ParameterName = "@InternationalCode"; cmd.Parameters.Add(pInternationalCode);

            var pFirs = cmd.CreateParameter(); pFirs.ParameterName = "@ForeignInvestmentRemainingShares"; cmd.Parameters.Add(pFirs);
            var pFis = cmd.CreateParameter(); pFis.ParameterName = "@ForeignInvestmentShares"; cmd.Parameters.Add(pFis);
            var pFirRatio = cmd.CreateParameter(); pFirRatio.ParameterName = "@ForeignInvestmentRemainRatio"; cmd.Parameters.Add(pFirRatio);
            var pFisRatio = cmd.CreateParameter(); pFisRatio.ParameterName = "@ForeignInvestmentSharesRatio"; cmd.Parameters.Add(pFisRatio);
            var pFiUpper = cmd.CreateParameter(); pFiUpper.ParameterName = "@ForeignInvestmentUpperLimitRatio"; cmd.Parameters.Add(pFiUpper);
            var pCnUpper = cmd.CreateParameter(); pCnUpper.ParameterName = "@ChineseInvestmentUpperLimitRatio"; cmd.Parameters.Add(pCnUpper);

            var pIssued = cmd.CreateParameter(); pIssued.ParameterName = "@NumberOfSharesIssued"; cmd.Parameters.Add(pIssued);
            var pDeclare = cmd.CreateParameter(); pDeclare.ParameterName = "@RecentlyDeclareDate"; cmd.Parameters.Add(pDeclare);
            var pNote = cmd.CreateParameter(); pNote.ParameterName = "@Note"; cmd.Parameters.Add(pNote);

            foreach (var x in list)
            {
                pStockId.Value = x.StockId;
                pDate.Value = x.Date;

                pStockName.Value = (object?)x.StockName ?? DBNull.Value;
                pInternationalCode.Value = (object?)x.InternationalCode ?? DBNull.Value;

                pFirs.Value = (object?)x.ForeignInvestmentRemainingShares ?? DBNull.Value;
                pFis.Value = (object?)x.ForeignInvestmentShares ?? DBNull.Value;
                pFirRatio.Value = (object?)x.ForeignInvestmentRemainRatio ?? DBNull.Value;
                pFisRatio.Value = (object?)x.ForeignInvestmentSharesRatio ?? DBNull.Value;
                pFiUpper.Value = (object?)x.ForeignInvestmentUpperLimitRatio ?? DBNull.Value;
                pCnUpper.Value = (object?)x.ChineseInvestmentUpperLimitRatio ?? DBNull.Value;

                pIssued.Value = (object?)x.NumberOfSharesIssued ?? DBNull.Value;
                pDeclare.Value = (object?)x.RecentlyDeclareDate ?? DBNull.Value;
                pNote.Value = (object?)x.Note ?? DBNull.Value;

                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
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
        private void EnsureShareholdingTable()
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

        public async Task<StockInfoDto?> GetStockInfoAsync(string stockId)
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

            if (!await reader.ReadAsync())
                return null;

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
        public async Task<List<string>> GetAllStockIdsAsync()
        {
            var result = new List<string>();

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT 公司代號
                FROM StockList
                ORDER BY 公司代號
            ";

            cmd.CommandText = @"
                SELECT StockId
                FROM ETFList
                ORDER BY StockId
            ";

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(reader.GetString(0));
            }
            return result;
        }
        string? EmptyToNull(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s;

        int ParseTwYear(string year)
        {
            // 98年 → 2009
            var n = int.Parse(year.Replace("年", ""));
            return n + 1911;
        }
        private static object DbValue(object? value)
        {
            return value ?? DBNull.Value;
        }

    }
}
