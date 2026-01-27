namespace Stock_Online.DTOs
{
    public class StockAnnualReviewDto
    {
        public int Year { get; set; }

        public decimal StartOpen { get; set; }
        public decimal EndClose { get; set; }

        public decimal CashDividend { get; set; }
        public decimal StockDividend { get; set; }

        public decimal CapitalGainRate { get; set; }   // %
        public decimal TotalReturnRate { get; set; }   // %
    }
}
