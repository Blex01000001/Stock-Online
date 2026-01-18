using Stock_Online.Domain.Entities;

namespace Stock_Online.Services.KLine.Indicators
{
    public interface IMovingAverageCalculator
    {
        public Dictionary<int, List<decimal?>> Calculate(List<StockDailyPrice> prices);
    }
}
