using Stock_Online.DTOs.Line_chart;

namespace Stock_Online.Services.Interface
{
    public interface IROILineChartService
    {
        Task<List<LineSeriesDto>> GetChart(string stockId, int year, int days);
    }
}
