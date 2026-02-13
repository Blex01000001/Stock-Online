namespace Stock_Online.DTOs
{
    public class InstitutionalDailyDto
    {
        public string Date { get; set; } = default!; // yyyy-MM-dd

        // 三大法人「總和」(net = buy-sell)
        public long ForeignNet { get; set; }
        public long InvestmentTrustNet { get; set; }
        public long DealerNet { get; set; }

        // ✅ 細項也保留（讓你前端可切換顯示）
        public long ForeignInvestorNet { get; set; }       // Foreign_Investor
        public long ForeignDealerSelfNet { get; set; }     // Foreign_Dealer_Self
        public long DealerSelfNet { get; set; }            // Dealer_self
        public long DealerHedgingNet { get; set; }         // Dealer_Hedging

        // （可選）合計
        public long TotalNet => ForeignNet + InvestmentTrustNet + DealerNet;
    }
}
