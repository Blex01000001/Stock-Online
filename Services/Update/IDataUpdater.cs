namespace Stock_Online.Services.Update
{
    public interface IDataUpdater
    {
        Task UpdateAsync(string stockId, DateTime date);
    }
}
