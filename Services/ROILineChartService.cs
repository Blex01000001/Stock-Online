using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using Stock_Online.DTOs.Line_chart;
using Stock_Online.Services.Interface;

namespace Stock_Online.Services
{
    public class ROILineChartService : IROILineChartService
    {
        private readonly IStockDailyPriceRepository _repo;

        public ROILineChartService(IStockDailyPriceRepository repo)
        {
            this._repo = repo;
        }
        public async Task<List<LineSeriesDto>> GetChart(string stockId, int year, int days)
        {
            Console.WriteLine($"GetChart {stockId} {year} {days}");
            List<StockDailyPrice> dailyPrices = await _repo.GetByStockIdAsync(stockId);
            StockDailyPrice[] prices = dailyPrices.OrderByDescending(x => x.TradeDate).ToArray();

            //for (int i = 0; i < prices.Length; i++)
            //{
            //    Console.WriteLine($"{i}\t{prices[i].TradeDate} {prices[i].ClosePrice}");
            //}
            Console.WriteLine($"66: {prices[66].TradeDate}");
            Console.WriteLine($"66-220: {prices[66+240].TradeDate}");

            var re = new List<LineSeriesDto>() { new LineSeriesDto()
            {
                Name = year.ToString(),
                Points = dailyPrices.Where(x => x.TradeDate > new DateTime(year, 1, 1))
                            .Select(x => new LinePointDto()
                            {
                                X = x.TradeDate.ToShortDateString(),
                                Y = (double)x.ClosePrice
                            }).ToList()
            } };
                
            return re;
        }
    }
}
