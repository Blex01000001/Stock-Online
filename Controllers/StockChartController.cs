using Microsoft.AspNetCore.Mvc;
using Stock_Online.Services.Interface;

namespace Stock_Online.Controllers
{
    [ApiController]
    [Route("chart")]
    public class StockChartController : ControllerBase
    {
        private readonly IROILineChartService _rOILineChartService;
        private readonly IKLineChartService _kLineChartService;
        public StockChartController(IROILineChartService rOILineChartService, IKLineChartService kLineChartService)
        {
            this._rOILineChartService = rOILineChartService;
            this._kLineChartService = kLineChartService;
        }
        [HttpGet("roi/line-chart")]
        public async Task<IActionResult> GetChart(
            [FromQuery] string stockId,
            [FromQuery] int year,
            [FromQuery] int days
)
        {
            Console.WriteLine($"{stockId} {year} {days}");
            if (string.IsNullOrWhiteSpace(stockId))
                return BadRequest("stockId is required");

            if (year <= 0)
                return BadRequest("year is invalid");

            if (days <= 0)
                return BadRequest("days must be greater than 0");

            return Ok(_rOILineChartService.GetChart(stockId, year, days));
        }
        /// <summary>
        /// K 線圖
        /// </summary>
        /// GET /chart/kline?stockId=2330&days=120
        /// GET /chart/kline?stockId=2330&start=20220101&end=20221231
        [HttpGet("kline")]
        public async Task<IActionResult> GetKLine(
            [FromQuery] string stockId,
            [FromQuery] int? days,
            [FromQuery] string? start,
            [FromQuery] string? end
        )
        {
            Console.WriteLine($"[KLine] {stockId} days={days} start={start} end={end}");

            if (string.IsNullOrWhiteSpace(stockId))
                return BadRequest("stockId is required");

            if (days.HasValue && days <= 0)
                return BadRequest("days must be greater than 0");

            if (!string.IsNullOrWhiteSpace(start) && start.Length != 8)
                return BadRequest("start format must be yyyyMMdd");

            if (!string.IsNullOrWhiteSpace(end) && end.Length != 8)
                return BadRequest("end format must be yyyyMMdd");

            var result = await _kLineChartService.GetKLineAsync(
                stockId,
                220,
                "20250101",
                "20261231"
            );
            Console.WriteLine($"result.Points.Count: {result.Points.Count}");
            return Ok(result);
        }

    }
}
