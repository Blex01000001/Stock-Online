using Stock_Online.Application.DTOs.ROILine;

namespace Stock_Online.Application.Services.ROILine
{
    public interface IROILineChartService
    {
        Task<List<LineSeriesDto>> GetChart(string stockId, int year, int days);
    }
}
