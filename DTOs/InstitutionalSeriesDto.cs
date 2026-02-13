namespace Stock_Online.DTOs
{
    public class InstitutionalSeriesDto
    {
        // 建議前端畫圖用這個：每個日期一筆，已對齊
        public List<InstitutionalDailyDto> Daily { get; set; } = new();

        // （可選）如果前端想直接畫細項，也可以有拆開的 series
        public Dictionary<string, List<InstitutionalPointDto>> RawSeries { get; set; } = new();
    }
}
