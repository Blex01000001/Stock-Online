using Stock_Online.Services.DataUpdater.Models.DTOs;

namespace Stock_Online.Services.DataUpdater.Models.Responses
{
    public class FinMindDividendResponse
    {
        public string msg { get; set; } = null!;
        public int status { get; set; }

        public List<FinMindDividendDto> data { get; set; } = new();
    }
}
