using Stock_Online.Domain.Interface;
using Stock_Online.DataAccess.SQLite.Interface;

namespace Stock_Online.Services.Update
{
    //public abstract class BaseUpdater<TEntity> : IDataUpdater where TEntity : class, IStockEntity
    //{
    //    protected readonly IStockRepository<TEntity> _repo;

    //    protected BaseUpdater(IStockRepository<TEntity> repository)
    //    {
    //        _repo = repository;
    //    }

    //    // 模板方法 (Template Method Pattern)
    //    public async Task UpdateAsync(string stockId, DateTime date)
    //    {
    //        // A. 抓取原始資料 (由子類別實作)
    //        string rawData = await FetchRawDataAsync(stockId, date);

    //        // B. 解析資料 (由子類別實作)
    //        IEnumerable<TEntity> entities = ParseData(rawData);

    //        // C. 寫入資料庫 (共通邏輯：Upsert)
    //        await _repo.UpsertAsync(entities);
    //    }

    //    protected abstract Task<string> FetchRawDataAsync(string stockId, DateTime date);
    //    protected abstract IEnumerable<TEntity> ParseData(string rawData);
    //}
}
