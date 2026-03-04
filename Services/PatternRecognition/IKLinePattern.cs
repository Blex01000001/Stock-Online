using Stock_Online.Domain.Entities;
using Stock_Online.Services.PatternRecognition.Models.DTOs;

namespace Stock_Online.Services.PatternRecognition
{
    public interface IKLinePattern
    {
        string Name { get; }
        // 傳入已還原的股價清單，回傳符合該型態的所有區間
        IEnumerable<PatternMatchResult> Match(List<StockDailyPrice> prices);
    }
}
