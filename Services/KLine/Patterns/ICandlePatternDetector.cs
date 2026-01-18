using Stock_Online.Domain.Entities;
using Stock_Online.Services.KLine.Patterns.Enum;

namespace Stock_Online.Services.KLine.Patterns
{
    public interface ICandlePatternDetector
    {
        bool IsMatch(CandlePattern pattern);
    }
}
