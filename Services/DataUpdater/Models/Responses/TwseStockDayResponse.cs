namespace Stock_Online.Services.DataUpdater.Models.Responses
{
    public class TwseStockDayResponse
    {
        public string stat { get; set; }
        public string date { get; set; }
        public List<List<string>> data { get; set; }
    }
}
