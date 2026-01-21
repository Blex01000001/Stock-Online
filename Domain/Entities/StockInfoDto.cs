namespace Stock_Online.Domain.Entities
{
    namespace Stock_Online.DTOs
    {
        public class StockInfoDto
        {
            public string StockId { get; set; } = default!;
            public string? CompanyName { get; set; }
            public string? CompanyShortName { get; set; }
            public string? Industry { get; set; }
            public string? Chairman { get; set; }
            public string? GeneralManager { get; set; }
            public string? Website { get; set; }
        }
    }
}
