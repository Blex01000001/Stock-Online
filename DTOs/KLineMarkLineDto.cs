namespace Stock_Online.DTOs
{
    public class KLineMarkLineDto
    {
        /// <summary>
        /// 日期（yyyy-MM-dd）
        /// </summary>
        public string Date { get; set; } = default!;

        /// <summary>
        /// 類型（Up / Down / Breakout / Custom）
        /// </summary>
        public string Type { get; set; } = default!;

        /// <summary>
        /// 顯示文字（tooltip 用）
        /// </summary>
        public string Label { get; set; } = default!;
    }
}
