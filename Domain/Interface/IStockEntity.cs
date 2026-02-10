namespace Stock_Online.Domain.Interface
{
    public interface IStockEntity
    {
        DateTime Date { get; set; }
        string StockId { get; set; }
    }
}
