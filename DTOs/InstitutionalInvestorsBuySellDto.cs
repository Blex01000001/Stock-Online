namespace Stock_Online.DTOs
{
    public class InstitutionalInvestorsBuySellDto
    {
        public string date { get; set; } = default!;
        public string stock_id { get; set; } = default!;
        public long buy { get; set; }
        public long sell { get; set; }
        public string name { get; set; } = default!;
    }
}
