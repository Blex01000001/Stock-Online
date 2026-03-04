namespace Stock_Online.Services.PatternRecognition.Models.DTOs
{
    public class PatternMatchResult
    {
        public string PatternName { get; set; } // 例如："吞噬型態"、"晨星"
        public int StartIndex { get; set; }    // 在 ChartData.Date 中的起始索引
        public int EndIndex { get; set; }      // 在 ChartData.Date 中的結束索引
        public string Signal { get; set; }     // "Bullish" (看多) 或 "Bearish" (看空)
    }
}
