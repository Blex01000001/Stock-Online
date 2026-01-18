using Stock_Online.DTOs;
using Stock_Online.Services.KLine.Patterns.Enum;

namespace Stock_Online.Services.KLine
{
    public interface IKLineChartService
    {
        Task<KLineChartDto> GetKLineAsync(
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
