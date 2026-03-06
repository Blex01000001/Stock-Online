namespace Stock_Online.Application.DTOs.KLine
{
    public class RsiDto
    {
        public List<double?> Rsi6 { get; set; } = new();
        public List<double?> Rsi12 { get; set; } = new();
        public List<double?> Rsi24 { get; set; } = new();
    }
}
