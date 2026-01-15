using SqlKata;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs.Line_chart;

namespace Stock_Online.Services
{
    public class CandlestickChartAnalysisService
    {
        private readonly IStockDailyPriceRepository _repo;
        private int _year;
        private List<StockDailyPrice> _orderedDailyPrices;
        public CandlestickChartAnalysisService(IStockDailyPriceRepository repo)
        {
            this._repo = repo;
        }
        public async Task GetChart(string stockId, int year, int days)
        {
            Console.WriteLine($"GetChart {stockId} {year} {days}");
            this._year = year;

            Query query = new Query("StockDailyPrice")
                .Where("StockId", stockId);

            List<StockDailyPrice> dailyPrices = await _repo.GetByQueryAsync(query);
            _orderedDailyPrices = dailyPrices.OrderByDescending(x => x.TradeDate).ToList();






            return ;
        }

    }
}
