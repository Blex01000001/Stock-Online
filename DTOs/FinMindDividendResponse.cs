namespace Stock_Online.DTOs
{
    public class FinMindDividendResponse
    {
        public string msg { get; set; } = null!;
        public int status { get; set; }

        public List<FinMindDividendDto> data { get; set; } = new();
    }
}
