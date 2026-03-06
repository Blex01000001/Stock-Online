using Stock_Online.Application.DTOs.DataUpdater;

namespace Stock_Online.Application.Services.DataUpdater.Models.Responses
{
    public class FinMindDividendResponse
    {
        public string msg { get; set; } = null!;
        public int status { get; set; }

        public List<FinMindDividendDto> data { get; set; } = new();
    }
}
