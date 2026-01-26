namespace Stock_Online.Services.Update
{
    public interface IStockDividendUpdateService
    {
        Task FetchAndSaveAllStockAsync();
    }
}
