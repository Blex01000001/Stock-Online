namespace Stock_Online.Application.DTOs.PatternRecognition
{
    public class StockPatternSummary
    {
        public string StockId { get; set; }
        public List<PatternMatchResult> Matches { get; set; }
    }
}
