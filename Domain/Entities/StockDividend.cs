namespace Stock_Online.Domain.Entities
{
    public class StockDividend
    {
        public string StockId { get; set; } = null!;
        public DateTime Date { get; set; }          // date
        public string Year { get; set; } = null!;   // 98年、99年

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
        public decimal CashIncreaseSubscriptionPrice { get; set; }

        public decimal RemunerationOfDirectorsAndSupervisors { get; set; }
        public decimal ParticipateDistributionOfTotalShares { get; set; }

        public string? AnnouncementDate { get; set; }
        public string? AnnouncementTime { get; set; }
    }
}
