using SqlKata;

namespace Stock_Online.Services.KLine.Queries
{
    public static class StockDailyPriceQueryBuilder
    {
        public static Query Build(
            string stockId, 
            int? days,
            string? start,
            string? end)
        {
            var q = new Query("StockDailyPrice")
                .Where("StockId", stockId);

            if (!string.IsNullOrWhiteSpace(start))
                q.Where("TradeDate", ">=", DateTime.ParseExact(start, "yyyyMMdd", null));

            if (!string.IsNullOrWhiteSpace(end))
                q.Where("TradeDate", "<=", DateTime.ParseExact(end, "yyyyMMdd", null));

            if (days.HasValue)
                q.OrderByDesc("TradeDate").Limit(days.Value);

            return q;
        }
    }
}
