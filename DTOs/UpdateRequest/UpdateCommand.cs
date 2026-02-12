namespace Stock_Online.DTOs.UpdateRequest
{
    // 定義維度的枚舉
    public enum StockScope { Single, Common, All } // 更新股票的範圍 單一 常用 全部
    public enum DataType { PriceUpdater, DividendUpdater, ShareHoldingUpdater } // 更新資料的類型
    public enum TimeScope { SpecificYear, FullHistory } // 更新時間 指定年 全部

    // 統一的更新請求指令
    public class UpdateCommand
    {
        public StockScope Scope { get; set; }
        public string? SpecificStockId { get; set; } // 僅當 Scope 為 Single 時使用

        public List<DataType> TargetDataTypes { get; set; } // 支援同時更新多種內容

        public TimeScope Time { get; set; }
        public int? SpecificYear { get; set; } // 僅當 Time 為 SpecificYear 時使用
    }
}
