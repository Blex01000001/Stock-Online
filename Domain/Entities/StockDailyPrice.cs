namespace Stock_Online.Domain.Entities
{
    public class StockDailyPrice
    {
        public string StockId { get; set; }
        public DateTime TradeDate { get; set; }
        public long Volume { get; set; }
        public long Amount { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal PriceChange { get; set; }
        public int TradeCount { get; set; }
        public string Note { get; set; }
    }
}
