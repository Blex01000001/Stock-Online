namespace Stock_Online.Services.PatternRecognition.Models.DTOs
{
    public class StockPatternSummary
    {
        public string StockId { get; set; }
        public List<PatternMatchResult> Matches { get; set; }
    }
}
