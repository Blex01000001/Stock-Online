using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
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
        private readonly SqliteBatchWriter _writer;
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly SqliteCompiler _compiler = new();
        public StockRepository(IConfiguration configuration /* or from DI */)
        {
            _connectionString = configuration.GetConnectionString("Sqlite")
                ?? throw new InvalidOperationException("Connection string not found.");

            _writer = new SqliteBatchWriter(_connectionString);
        }
        public Task<List<StockDailyPrice>> GetPricesAsync(Query query)
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
        public Task<List<StockCorporateAction>> GetCorporateActionsAsync(Query query)
        {
            // ✅ 建議：如果你希望強制排序，就在這裡補 OrderBy
            // query = query.Clone().OrderByDesc("ExDate");  // 若你有 Clone 擴充
            // 沒 Clone 的話就請呼叫端自己加 OrderByDesc("ExDate")

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

        //public async Task<List<StockDailyPrice>> GetPriceByQueryAsync(Query query)
        //{
        //    var result = new List<StockDailyPrice>();

        //    // 1️⃣ 編譯 SqlKata → SQL + Parameters
        //    var compiled = _compiler.Compile(query);

        //    await using var conn = new SqliteConnection(_connectionString);
        //    await conn.OpenAsync();

        //    await using var cmd = conn.CreateCommand();
        //    cmd.CommandText = compiled.Sql;

        //    // 2️⃣ 參數綁定（這一步很關鍵）
        //    foreach (var kv in compiled.NamedBindings)
        //    {
        //        cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
        //    }

        //    // 3️⃣ Execute + Mapping
        //    await using var reader = await cmd.ExecuteReaderAsync();
        //    while (await reader.ReadAsync())
        //    {
        //        result.Add(new StockDailyPrice
        //        {
        //            StockId = reader.GetString(reader.GetOrdinal("StockId")),

        //            TradeDate = DateTime.ParseExact(
        //                reader.GetString(reader.GetOrdinal("TradeDate")),
        //                "yyyy-MM-dd",
        //                CultureInfo.InvariantCulture
        //            ),

        //            Volume = reader.IsDBNull(reader.GetOrdinal("Volume")) ? 0 : reader.GetInt64(reader.GetOrdinal("Volume")),
        //            Amount = reader.IsDBNull(reader.GetOrdinal("Amount")) ? 0 : reader.GetInt64(reader.GetOrdinal("Amount")),

        //            OpenPrice = reader.IsDBNull(reader.GetOrdinal("OpenPrice")) ? 0 : reader.GetDecimal(reader.GetOrdinal("OpenPrice")),
        //            HighPrice = reader.IsDBNull(reader.GetOrdinal("HighPrice")) ? 0 : reader.GetDecimal(reader.GetOrdinal("HighPrice")),
        //            LowPrice = reader.IsDBNull(reader.GetOrdinal("LowPrice")) ? 0 : reader.GetDecimal(reader.GetOrdinal("LowPrice")),
        //            ClosePrice = reader.IsDBNull(reader.GetOrdinal("ClosePrice")) ? 0 : reader.GetDecimal(reader.GetOrdinal("ClosePrice")),

        //            PriceChange = reader.IsDBNull(reader.GetOrdinal("PriceChange")) ? 0 : reader.GetDecimal(reader.GetOrdinal("PriceChange")),
        //            TradeCount = reader.IsDBNull(reader.GetOrdinal("TradeCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("TradeCount")),

        //            Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? null : reader.GetString(reader.GetOrdinal("Note"))
        //        });
        //    }

        //    return result;
        //}
        //public async Task<List<StockDividend>> GetDividendByQueryAsync(Query query)
        //{
        //    var result = new List<StockDividend>();

        //    // 1️⃣ 編譯 SqlKata → SQL + Parameters
        //    var compiled = _compiler.Compile(query);

        //    await using var conn = new SqliteConnection(_connectionString);
        //    await conn.OpenAsync();

        //    await using var cmd = conn.CreateCommand();
        //    cmd.CommandText = compiled.Sql;

        //    // 2️⃣ 參數綁定
        //    foreach (var kv in compiled.NamedBindings)
        //    {
        //        cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
        //    }

        //    // 3️⃣ Execute + Mapping
        //    await using var reader = await cmd.ExecuteReaderAsync();
        //    while (await reader.ReadAsync())
        //    {
        //        result.Add(new StockDividend
        //        {
        //            StockId = reader.GetString(reader.GetOrdinal("StockId")),

        //            Date = DateTime.ParseExact(
        //                reader.GetString(reader.GetOrdinal("Date")),
        //                "yyyy-MM-dd",
        //                CultureInfo.InvariantCulture
        //            ),

        //            Year = reader.GetString(reader.GetOrdinal("Year")),

        //            StockEarningsDistribution =
        //                reader.IsDBNull(reader.GetOrdinal("StockEarningsDistribution"))
        //                    ? null
        //                    : reader.GetDecimal(reader.GetOrdinal("StockEarningsDistribution")),

        //            StockStatutorySurplus =
        //                reader.IsDBNull(reader.GetOrdinal("StockStatutorySurplus"))
        //                    ? null
        //                    : reader.GetDecimal(reader.GetOrdinal("StockStatutorySurplus")),

        //            StockExDividendTradingDate =
        //                reader.IsDBNull(reader.GetOrdinal("StockExDividendTradingDate"))
        //                    ? null
        //                    : reader.GetString(reader.GetOrdinal("StockExDividendTradingDate")),

        //            TotalEmployeeStockDividend =
        //                reader.IsDBNull(reader.GetOrdinal("TotalEmployeeStockDividend"))
        //                    ? null
        //                    : reader.GetDecimal(reader.GetOrdinal("TotalEmployeeStockDividend")),

        //            TotalEmployeeStockDividendAmount =
        //                reader.IsDBNull(reader.GetOrdinal("TotalEmployeeStockDividendAmount"))
        //                    ? null
        //                    : reader.GetDecimal(reader.GetOrdinal("TotalEmployeeStockDividendAmount")),

        //            RatioOfEmployeeStockDividendOfTotal =
        //                reader.IsDBNull(reader.GetOrdinal("RatioOfEmployeeStockDividendOfTotal"))
        //                    ? null
        //                    : reader.GetDecimal(reader.GetOrdinal("RatioOfEmployeeStockDividendOfTotal")),

        //            RatioOfEmployeeStockDividend =
        //                reader.IsDBNull(reader.GetOrdinal("RatioOfEmployeeStockDividend"))
        //                    ? null
        //                    : reader.GetDecimal(reader.GetOrdinal("RatioOfEmployeeStockDividend")),

        //            CashEarningsDistribution =
        //                reader.IsDBNull(reader.GetOrdinal("CashEarningsDistribution"))
        //                    ? null
        //                    : reader.GetDecimal(reader.GetOrdinal("CashEarningsDistribution")),

        //            CashStatutorySurplus =
        //                reader.IsDBNull(reader.GetOrdinal("CashStatutorySurplus"))
        //                    ? null
        //                    : reader.GetDecimal(reader.GetOrdinal("CashStatutorySurplus")),

        //            CashExDividendTradingDate =
        //                reader.IsDBNull(reader.GetOrdinal("CashExDividendTradingDate"))
        //                    ? null
        //                    : reader.GetString(reader.GetOrdinal("CashExDividendTradingDate")),

        //            CashDividendPaymentDate =
        //                reader.IsDBNull(reader.GetOrdinal("CashDividendPaymentDate"))
        //                    ? null
        //                    : reader.GetString(reader.GetOrdinal("CashDividendPaymentDate")),

        //            TotalEmployeeCashDividend =
        //                reader.IsDBNull(reader.GetOrdinal("TotalEmployeeCashDividend"))
        //                    ? null
        //                    : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("TotalEmployeeCashDividend"))),

        //            TotalNumberOfCashCapitalIncrease =
        //                reader.IsDBNull(reader.GetOrdinal("TotalNumberOfCashCapitalIncrease"))
        //                    ? null
        //                    : reader.GetDecimal(reader.GetOrdinal("TotalNumberOfCashCapitalIncrease")),

        //            CashIncreaseSubscriptionRate =
        //                reader.IsDBNull(reader.GetOrdinal("CashIncreaseSubscriptionRate"))
        //                    ? null
        //                    : reader.GetDecimal(reader.GetOrdinal("CashIncreaseSubscriptionRate")),

        //            CashIncreaseSubscriptionPrice =
        //                reader.IsDBNull(reader.GetOrdinal("CashIncreaseSubscriptionPrice"))
        //                    ? null
        //                    : reader.GetDecimal(reader.GetOrdinal("CashIncreaseSubscriptionPrice")),

        //            RemunerationOfDirectorsAndSupervisors =
        //                reader.IsDBNull(reader.GetOrdinal("RemunerationOfDirectorsAndSupervisors"))
        //                    ? null
        //                    : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("RemunerationOfDirectorsAndSupervisors"))),

        //            ParticipateDistributionOfTotalShares =
        //                reader.IsDBNull(reader.GetOrdinal("ParticipateDistributionOfTotalShares"))
        //                    ? null
        //                    : reader.GetDecimal(reader.GetOrdinal("ParticipateDistributionOfTotalShares")),

        //            AnnouncementDate =
        //                reader.IsDBNull(reader.GetOrdinal("AnnouncementDate"))
        //                    ? null
        //                    : reader.GetString(reader.GetOrdinal("AnnouncementDate")),

        //            AnnouncementTime =
        //                reader.IsDBNull(reader.GetOrdinal("AnnouncementTime"))
        //                    ? null
        //                    : reader.GetString(reader.GetOrdinal("AnnouncementTime"))
        //        });
        //    }

        //    return result;
        //}
        //public async Task<List<StockShareholding>> GetShareholdingByQueryAsync(Query query)
        //{
        //    var result = new List<StockShareholding>();

        //    // 1️⃣ 編譯 SqlKata → SQL + Parameters
        //    var compiled = _compiler.Compile(query);

        //    await using var conn = new SqliteConnection(_connectionString);
        //    await conn.OpenAsync();

        //    await using var cmd = conn.CreateCommand();
        //    cmd.CommandText = compiled.Sql;

        //    // 2️⃣ 參數綁定
        //    foreach (var kv in compiled.NamedBindings)
        //    {
        //        cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
        //    }

        //    // 3️⃣ Execute + Mapping
        //    await using var reader = await cmd.ExecuteReaderAsync();
        //    while (await reader.ReadAsync())
        //    {
        //        result.Add(new StockShareholding
        //        {
        //            StockId = reader.GetString(reader.GetOrdinal("StockId")),
        //            Date = reader.GetString(reader.GetOrdinal("Date")),

        //            StockName = reader.IsDBNull(reader.GetOrdinal("StockName"))
        //                ? null
        //                : reader.GetString(reader.GetOrdinal("StockName")),

        //            InternationalCode = reader.IsDBNull(reader.GetOrdinal("InternationalCode"))
        //                ? null
        //                : reader.GetString(reader.GetOrdinal("InternationalCode")),

        //            ForeignInvestmentRemainingShares = reader.IsDBNull(reader.GetOrdinal("ForeignInvestmentRemainingShares"))
        //                ? null
        //                : Convert.ToInt64(reader.GetValue(reader.GetOrdinal("ForeignInvestmentRemainingShares"))),

        //            ForeignInvestmentShares = reader.IsDBNull(reader.GetOrdinal("ForeignInvestmentShares"))
        //                ? null
        //                : Convert.ToInt64(reader.GetValue(reader.GetOrdinal("ForeignInvestmentShares"))),

        //            ForeignInvestmentRemainRatio = reader.IsDBNull(reader.GetOrdinal("ForeignInvestmentRemainRatio"))
        //                ? null
        //                : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("ForeignInvestmentRemainRatio"))),

        //            ForeignInvestmentSharesRatio = reader.IsDBNull(reader.GetOrdinal("ForeignInvestmentSharesRatio"))
        //                ? null
        //                : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("ForeignInvestmentSharesRatio"))),

        //            ForeignInvestmentUpperLimitRatio = reader.IsDBNull(reader.GetOrdinal("ForeignInvestmentUpperLimitRatio"))
        //                ? null
        //                : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("ForeignInvestmentUpperLimitRatio"))),

        //            ChineseInvestmentUpperLimitRatio = reader.IsDBNull(reader.GetOrdinal("ChineseInvestmentUpperLimitRatio"))
        //                ? null
        //                : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("ChineseInvestmentUpperLimitRatio"))),

        //            NumberOfSharesIssued = reader.IsDBNull(reader.GetOrdinal("NumberOfSharesIssued"))
        //                ? null
        //                : Convert.ToInt64(reader.GetValue(reader.GetOrdinal("NumberOfSharesIssued"))),

        //            RecentlyDeclareDate = reader.IsDBNull(reader.GetOrdinal("RecentlyDeclareDate"))
        //                ? null
        //                : reader.GetString(reader.GetOrdinal("RecentlyDeclareDate")),

        //            Note = reader.IsDBNull(reader.GetOrdinal("Note"))
        //                ? null
        //                : reader.GetString(reader.GetOrdinal("Note")),
        //        });
        //    }

        //    return result;
        //}
        //public async Task<List<StockCorporateAction>> GetCorporateActionsAsync(string stockId)
        //{
        //    var result = new List<StockCorporateAction>();

        //    await using var conn = new SqliteConnection(_connectionString);
        //    await conn.OpenAsync();

        //    await using var cmd = conn.CreateCommand();
        //    cmd.CommandText = @"
        //        SELECT
        //            StockId,
        //            ActionType,
        //            ExDate,
        //            Ratio,
        //            CashAmount,
        //            Description
        //        FROM StockCorporateAction
        //        WHERE StockId = @stockId
        //        ORDER BY ExDate DESC
        //    ";

        //    cmd.Parameters.AddWithValue("@stockId", stockId);

        //    await using var reader = await cmd.ExecuteReaderAsync();
        //    while (await reader.ReadAsync())
        //    {
        //        result.Add(new StockCorporateAction
        //        {
        //            StockId = reader.GetString(0),
        //            ActionType = Enum.Parse<CorporateActionType>(reader.GetString(1)),
        //            ExDate = DateTime.Parse(reader.GetString(2)),
        //            Ratio = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
        //            CashAmount = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
        //            Description = reader.IsDBNull(5) ? null : reader.GetString(5)
        //        });
        //    }

        //    return result;
        //}

        public async Task SavePriceToDbAsync(List<StockDailyPrice> list)
        {
            const string sql = """
                INSERT OR REPLACE INTO StockDailyPrice
                (StockId, TradeDate, Volume, Amount, OpenPrice, HighPrice, LowPrice,
                 ClosePrice, PriceChange, TradeCount, Note)
                VALUES
                (@StockId, @TradeDate, @Volume, @Amount, @Open, @High, @Low,
                 @Close, @Change, @Count, @Note);
                """;

            await _writer.ExecuteAsync(
                list,
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
        public async Task SaveDividendToDbAsync(List<StockDividend> list)
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
                list,
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
        public async Task SaveShareholdingToDb(List<StockShareholding> list)
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
                list,
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
        private async Task<List<T>> QueryAsync<T>(
            Query query,
            Func<SqliteDataReader, T> map)
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
    }
}
