namespace Stock_Online.DTOs
{
    public class KLineChartDto
    {
        public string StockId { get; set; }

        public List<KLinePointDto> Points { get; set; } = new();

        /// <summary>
        /// MA 線集合
        /// </summary>
        public List<MALineDto> MALines { get; set; } = new();
    }
}
