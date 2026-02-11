using Stock_Online.DTOs.UpdateRequest;

namespace Stock_Online.Services.DataUpdater
{
    public interface IDataUpdater
    {
        DataType DataType { get; }
        Task UpdateAsync(string stockId, int year); // 核心更新邏輯
    }
}
