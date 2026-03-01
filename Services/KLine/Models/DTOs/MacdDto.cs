namespace Stock_Online.Services.KLine.Models.DTOs
{
    public class MacdDto
    {
        // DIF 快線 (通常是 EMA12 - EMA26)
        public List<decimal?> Dif { get; set; } = new();

        // DEA 慢線 (DIF 的 EMA9)
        public List<decimal?> Dea { get; set; } = new();

        // MACD 柱狀圖 (DIF - DEA) * 2
        // 乘 2 是為了讓視覺上柱狀圖更明顯，這是許多看盤軟體的做法
        public List<decimal?> Hist { get; set; } = new();
    }
}
