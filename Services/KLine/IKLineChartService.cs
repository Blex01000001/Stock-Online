using Stock_Online.Domain.Enums;
using Stock_Online.DTOs;

namespace Stock_Online.Services.KLine
{
    public interface IKLineChartService
    {
        Task<List<KLineChartDto>> GetKLineAsync(
            string stockId,
            int? days,
            string? start,
            string? end
        );
        Task<List<KLineChartDto>> GetKPatternLineAsync(
            string stockId,
            CandlePattern candlePattern
        );
    }
}
