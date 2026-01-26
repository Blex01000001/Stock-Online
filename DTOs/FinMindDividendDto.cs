namespace Stock_Online.DTOs
{
    public class FinMindDividendDto
    {
        public string date { get; set; } = null!;
        public string stock_id { get; set; } = null!;
        public string year { get; set; } = null!;

        public decimal StockEarningsDistribution { get; set; }
        public decimal StockStatutorySurplus { get; set; }
        public string? StockExDividendTradingDate { get; set; }

        public decimal TotalEmployeeStockDividend { get; set; }
        public decimal TotalEmployeeStockDividendAmount { get; set; }
        public decimal RatioOfEmployeeStockDividendOfTotal { get; set; }
        public decimal RatioOfEmployeeStockDividend { get; set; }

        public decimal CashEarningsDistribution { get; set; }
        public decimal CashStatutorySurplus { get; set; }
        public string? CashExDividendTradingDate { get; set; }
        public string? CashDividendPaymentDate { get; set; }

        public decimal TotalEmployeeCashDividend { get; set; }
        public decimal TotalNumberOfCashCapitalIncrease { get; set; }
        public decimal CashIncreaseSubscriptionRate { get; set; }
        public decimal CashIncreaseSubscriptionpRrice { get; set; }

        public decimal RemunerationOfDirectorsAndSupervisors { get; set; }
        public decimal ParticipateDistributionOfTotalShares { get; set; }

        public string? AnnouncementDate { get; set; }
        public string? AnnouncementTime { get; set; }
    }
}
