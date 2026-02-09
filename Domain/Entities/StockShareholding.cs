namespace Stock_Online.Domain.Entities
{
    public sealed class StockShareholding
    {
        public string StockId { get; set; } = "";
        public string Date { get; set; } = ""; // 建議存 "yyyy-MM-dd" 或 "yyyyMMdd" 其中一種，全文一致

        public string? StockName { get; set; }
        public string? InternationalCode { get; set; }

        public long? ForeignInvestmentRemainingShares { get; set; }
        public long? ForeignInvestmentShares { get; set; }
        public decimal? ForeignInvestmentRemainRatio { get; set; }
        public decimal? ForeignInvestmentSharesRatio { get; set; }
        public decimal? ForeignInvestmentUpperLimitRatio { get; set; }
        public decimal? ChineseInvestmentUpperLimitRatio { get; set; }
        public long? NumberOfSharesIssued { get; set; }

        public string? RecentlyDeclareDate { get; set; }
        public string? Note { get; set; }
    }
}
