using Stock_Online.Domain.Entities;
using Stock_Online.Domain.Enums;
using System.Diagnostics;
using System;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.InteropServices;

namespace Stock_Online.Services.KLine.Patterns
{
    public class CandlePatternDetector : ICandlePatternDetector
    {
        private readonly IReadOnlyList<StockDailyPrice> _prices;
        private readonly Dictionary<int, List<decimal?>> _maMap;
        private readonly int _index;

        public CandlePatternDetector(IReadOnlyList<StockDailyPrice> prices, int index, Dictionary<int, List<decimal?>> maMap)
        {
            _prices = prices ?? throw new ArgumentNullException(nameof(prices));
            _index = index;
            _maMap = maMap;
        }
        public bool IsMatch(CandlePattern pattern)
        {
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //Type type = assembly.GetType($"Stock_Online.Services.KLine.Patterns.Items.{pattern.ToString()}");
            //ICandlePattern candlePattern = (ICandlePattern)Activator.CreateInstance(type);
            ICandlePattern candlePattern = CandlePatternFactory.Get(pattern.ToString());
            CandleContext ctx = new CandleContext(_prices, _index, _maMap);
            return candlePattern.IsMatch(ctx);
        }
    }
}
