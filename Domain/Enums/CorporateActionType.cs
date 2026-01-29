namespace Stock_Online.Domain.Enums
{
    public enum CorporateActionType
    {
        Split,          // 股票分割 (1→4)
        StockDividend,  // 配股
        CashDividend,   // 現金股利
        Reduction,      // 減資
        Merge           // 合併
    }
}
