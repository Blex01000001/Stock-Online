namespace Stock_Online.Domain.Entities
{
    public class StockInstitutionalInvestorsBuySell
    {
        public string StockId { get; set; } = default!;
        public string Date { get; set; } = default!; // 先跟你 Shareholding 同樣用 string "yyyy-MM-dd"
        public string Name { get; set; } = default!;

        public long Buy { get; set; }
        public long Sell { get; set; }

        public long NetBuySell => Buy - Sell;
    }
}
