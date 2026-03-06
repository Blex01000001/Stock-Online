using Stock_Online.Application.DTOs.Commands;

namespace Stock_Online.Application.Services.DataUpdater
{
    public interface IDataUpdater
    {
        DataType DataType { get; }
        Task UpdateAsync(string stockId, int year); // 核心更新邏輯
    }
}
