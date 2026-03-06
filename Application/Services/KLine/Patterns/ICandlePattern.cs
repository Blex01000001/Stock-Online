using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Enums;

namespace Stock_Online.Application.Services.KLine.Patterns
{
    public interface ICandlePattern
    {
        public abstract bool IsMatch(CandleContext ctx);
    }
}
