using SqlKata;
using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using Stock_Online.DTOs.Line_chart;
using System;

namespace Stock_Online.Services.ROILine
{
    public class ROILineChartService : IROILineChartService
    {
        private readonly IStockRepository _repo;
        private int _year;
        private List<StockDailyPrice> _orderedDailyPrices;

        public ROILineChartService(IStockRepository repo)
        {
            _repo = repo;
        }
        public async Task<List<LineSeriesDto>> GetChart(string stockId, int year, int days)
        {
            Console.WriteLine($"GetChart {stockId} {year} {days}");
            _year = year;

            Query query = new Query("StockDailyPrice")
                .Where("StockId", stockId);

            List<StockDailyPrice> dailyPrices = await _repo.GetPricesAsync(query);

            _orderedDailyPrices = dailyPrices.OrderByDescending(x => x.TradeDate).ToList();

            var lineSeriesDto = new List<LineSeriesDto>() {
                CreateLineSeries(days),
                CreateLineSeries(days*2),
                CreateLineSeries(days*3),
                CreateLineSeries(days*5),
                CreateLineSeries(days*10),
            };

            return lineSeriesDto;
        }
        private LineSeriesDto CreateLineSeries(int days)
        {
            return new LineSeriesDto()
            {
                Name = days.ToString(),
                Points = _orderedDailyPrices.OrderBy(x => x.TradeDate).Where(x => x.TradeDate > new DateTime(_year, 1, 1))
                    .Select(x => new LinePointDto()
                    {
                        X = x.TradeDate.ToShortDateString(),
                        Y = (double)(x.ClosePrice / _orderedDailyPrices[_orderedDailyPrices.FindIndex(y => y.TradeDate == x.TradeDate) + days].ClosePrice)
                    }).ToList()
            };
        }

    }
}
