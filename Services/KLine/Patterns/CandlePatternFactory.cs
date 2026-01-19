using System.Reflection;

namespace Stock_Online.Services.KLine.Patterns
{
    public static class CandlePatternFactory
    {
        private static readonly Dictionary<string, ICandlePattern> _cache;

        static CandlePatternFactory()
        {
            _cache = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(ICandlePattern).IsAssignableFrom(t) && !t.IsAbstract)
                .ToDictionary(
                    t => t.Name,
                    t => (ICandlePattern)Activator.CreateInstance(t)
                );
        }

        public static ICandlePattern Get(string patternName)
            => _cache.TryGetValue(patternName, out var p)
                ? p
                : throw new ArgumentException($"Unknown pattern: {patternName}");
    }
}
