using Stock_Online.Services.ROILine.Models.DTOs;

namespace Stock_Online.Services.ROILine
{
    public interface IROILineChartService
    {
        Task<List<LineSeriesDto>> GetChart(string stockId, int year, int days);
    }
}
