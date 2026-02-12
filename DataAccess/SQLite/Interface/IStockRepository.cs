using SqlKata;
using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Entities.Stock_Online.DTOs;

namespace Stock_Online.DataAccess.SQLite.Interface
{
    public interface IStockRepository
    {
        Task<List<StockCorporateAction>> GetCorporateActionsAsync(Query query);
        Task<List<StockDailyPrice>> GetPricesAsync(Query query);
        Task<List<StockDividend>> GetDividendsAsync(Query query);
        Task<List<StockShareholding>> GetShareHoldingsAsync(Query query);
        Task<List<StockInstitutionalInvestorsBuySell>> GetInstitutionalInvestorsBuySellAsync(Query query);
        Task<StockInfoDto?> GetStockInfoAsync(string stockId);
        Task<List<string>> GetAllStockIdsAsync();
        Task SavePriceToDbAsync(List<StockDailyPrice> list);
        Task SaveDividendToDbAsync(List<StockDividend> list);
        Task SaveShareholdingToDb(List<StockShareholding> list);
        Task SaveInstitutionalInvestorsBuySellToDb(List<StockInstitutionalInvestorsBuySell> list);
    }
}
