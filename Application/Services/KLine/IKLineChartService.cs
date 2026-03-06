using Stock_Online.Application.DTOs.KLine;
using Stock_Online.Domain.Enums;

namespace Stock_Online.Application.Services.KLine
{
    public interface IKLineChartService
    {
        Task<List<KLineChartDto>> GetKLineAsync(
            string stockId,
            int? days,
            string? start,
            string? end
        );
    }
}
