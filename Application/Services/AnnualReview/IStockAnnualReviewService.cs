using Stock_Online.Application.DTOs.AnnualReview;

namespace Stock_Online.Application.Services.AnnualReview
{
    public interface IStockAnnualReviewService
    {
        Task<List<StockAnnualReviewDto>> GetDataAsync(string stockId);
    }
}
