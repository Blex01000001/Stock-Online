using Stock_Online.Domain.Entities;

namespace Stock_Online.DTOs

{
    public class KLineChartDto
    {
        public string StockId { get; set; }

        public List<KLinePointDto> Points { get; set; } = new();
        public string PatternType { get; set; }

        /// <summary>
        /// MA 線集合
        /// </summary>
        public List<MALineDto> MALines { get; set; } = new();
        public List<KLineMarkerDto> Markers { get; set; } = new();
        public List<KLineMarkLineDto> MarkLines { get; set; } = new();
        public List<StockShareholding> Shareholdings { get; set; } = new();
        public InstitutionalSeriesDto Institutional { get; set; } = new();
    }
}
