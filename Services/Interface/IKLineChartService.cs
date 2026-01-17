using Stock_Online.DTOs;

namespace Stock_Online.Services.Interface
{
    public interface IKLineChartService
    {
        Task<KLineChartDto> GetKLineAsync(
            string stockId,
            int? days,
            string? start,
            string? end
        );
        Task<List<KLineChartDto>> GetKMultipleLineAsync(
            string stockId,
            int? days,
            string? start,
            string? end
        );

    }
}
