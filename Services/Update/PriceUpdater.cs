using Stock_Online.DataAccess.SQLite.Interface;
using Stock_Online.Domain.Entities;

namespace Stock_Online.Services.Update
{
    //public class PriceUpdater : BaseUpdater<StockDailyPrice>
    //{
    //    public PriceUpdater(IStockRepository<StockDailyPrice> repo) : base(repo) { }

    //    protected override async Task<string> FetchRawDataAsync(string stockId, DateTime date)
    //    {
    //        if (string.IsNullOrWhiteSpace(stockId)) throw new ArgumentException("StockId 不可為空");
    //        var startTime = DateTime.Now;
    //        bool success;

    //        try
    //        {
    //            success = await FetchAndSaveWithRetryAsync(date.Year, stockId);
    //        }
    //        catch (Exception ex)
    //        {
    //            //jobLog.SingleStockFail(stockId, ex);
    //            throw;
    //        }

    //        if (!success)
    //        {
    //            //jobLog.SingleStockFail(
    //            //    stockId,
    //            //    new Exception("Update failed after 3 retries")
    //            //);
    //            throw new Exception($"Stock {stockId} update failed");
    //        }
    //    }



    //    protected override IEnumerable<StockDailyPrice> ParseData(string rawData)
    //    {
    //        // 實作 JSON/CSV 解析邏輯...
    //        return new List<StockDailyPrice>();
    //    }



    //}
}
