namespace Stock_Online.Services.Update
{
    public interface IStockShareholdingUpdateService
    {
        Task FetchAndSaveAsync(string startDate);
    }
}
