namespace Stock_Online.DTOs.Line_chart
{
    public class LineSeriesDto
    {
        public string Name { get; set; } = "";   // 線的名稱（2330、MA20、成本A）
        public List<LinePointDto> Points { get; set; } = new();
    }
}
