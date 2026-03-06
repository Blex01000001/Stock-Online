using SqlKata;
using Stock_Online.Domain.Entities;

namespace Stock_Online.Domain.Interfaces.Repositories
{
    public interface IInstitutionalInvestorRepository
    {
        Task<List<StockInstitutionalInvestorsBuySell>> GetDailyTradesAsync(Query query);
        Task UpsertDailyTradesAsync(List<StockInstitutionalInvestorsBuySell> trades);
        Task<List<StockShareholding>> GetShareHoldingsAsync(Query query);
        Task UpsertGetShareHoldingsAsync(List<StockShareholding> ownerships);
    }
}
