using Stock_Online.DTOs;

namespace Stock_Online.Services.AnnualReview
{
    public interface IStockAnnualReviewService
    {
        Task<List<StockAnnualReviewDto>> GetDataAsync(string stockId);
    }
}
