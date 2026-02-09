namespace Stock_Online.DTOs
{
    public sealed class TaiwanStockShareholdingDto
    {
        public string date { get; set; } = "";               // "2026-02-02"
        public string stock_id { get; set; } = "";           // "2330"
        public string stock_name { get; set; } = "";         // "\u53f0\u7a4d\u96fb"
        public string InternationalCode { get; set; } = "";

        public long? ForeignInvestmentRemainingShares { get; set; }
        public long? ForeignInvestmentShares { get; set; }
        public decimal? ForeignInvestmentRemainRatio { get; set; }
        public decimal? ForeignInvestmentSharesRatio { get; set; }
        public decimal? ForeignInvestmentUpperLimitRatio { get; set; }
        public decimal? ChineseInvestmentUpperLimitRatio { get; set; }
        public long? NumberOfSharesIssued { get; set; }

        public string RecentlyDeclareDate { get; set; } = "";
        public string note { get; set; } = "";
    }
}
