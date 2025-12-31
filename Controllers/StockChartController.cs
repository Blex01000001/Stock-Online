using Microsoft.AspNetCore.Mvc;
using Stock_Online.Services.Interface;

namespace Stock_Online.Controllers
{
    [ApiController]
    [Route("chart")]
    public class StockChartController : ControllerBase
    {
        private readonly IROILineChartService _rOILineChartService;
        public StockChartController(IROILineChartService rOILineChartService)
        {
            this._rOILineChartService = rOILineChartService;
        }
        [HttpGet("roi/line-chart")]
        public async Task<IActionResult> GetChart(
            [FromQuery] string stockId,
            [FromQuery] int year,
            [FromQuery] int days
)
        {
            if (string.IsNullOrWhiteSpace(stockId))
                return BadRequest("stockId is required");

            if (year <= 0)
                return BadRequest("year is invalid");

            if (days <= 0)
                return BadRequest("days must be greater than 0");

            return Ok(_rOILineChartService.GetChart(stockId, year, days));
        }

    }
}
