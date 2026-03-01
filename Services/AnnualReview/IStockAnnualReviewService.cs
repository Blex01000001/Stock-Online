using Stock_Online.Services.AnnualReview.Models.DTOs;

namespace Stock_Online.Services.AnnualReview
{
    public interface IStockAnnualReviewService
    {
        Task<List<StockAnnualReviewDto>> GetDataAsync(string stockId);
    }
}
