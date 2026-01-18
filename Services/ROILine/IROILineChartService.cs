using Stock_Online.DTOs.Line_chart;

namespace Stock_Online.Services.ROILine
{
    public interface IROILineChartService
    {
        Task<List<LineSeriesDto>> GetChart(string stockId, int year, int days);
    }
}
