namespace Stock_Online.DTOs
{
    public class KLineMarkerDto
    {
        /// <summary>
        /// 日期（yyyy-MM-dd）
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 類型：Selected / Hammer / Signal / Custom
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 顯示文字
        /// </summary>
        public string Label { get; set; }
    }
}
