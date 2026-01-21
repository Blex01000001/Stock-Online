namespace Stock_Online.Common.Validation
{
    public static class YearValidator
    {
        public static (bool IsValid, string? Error) Validate(
            int year,
            int minYear = 2010)
        {
            int currentYear = DateTime.Now.Year;

            if (year <= 0)
                return (false, "year 不可為空或為 0");

            if (year < 1911)
                return (false, "請輸入西元年（例如 2025）");

            if (year > currentYear)
                return (false, $"year 不可大於今年（{currentYear}）");

            if (year < minYear)
                return (false, $"year 不可小於 {minYear}");

            return (true, null);
        }
    }
}
