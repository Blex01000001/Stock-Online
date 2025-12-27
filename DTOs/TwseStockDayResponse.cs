namespace Stock_Online.DTOs
{
    public class TwseStockDayResponse
    {
        public string stat { get; set; }
        public string date { get; set; }
        public List<List<string>> data { get; set; }
    }
}
