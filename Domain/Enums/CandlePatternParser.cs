namespace Stock_Online.Domain.Enums
{
    public static class CandlePatternParser
    {
        public static bool TryParse(string? value, out CandlePattern pattern)
        {
            pattern = default;

            if (string.IsNullOrWhiteSpace(value))
                return false;
            
            return Enum.TryParse(
                value,
                ignoreCase: true,
                out pattern
            );
        }
    }
}
