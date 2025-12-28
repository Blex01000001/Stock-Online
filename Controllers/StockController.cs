using Microsoft.AspNetCore.Mvc;
using Stock_Online.Services;
using Stock_Online.DTOs;
using Stock_Online.Services.Interface;

namespace Stock_Online.Controllers
{
    [ApiController]
    [Route("stock")]
    public class StockController : Controller
    {
        private readonly IStockDailyPriceService _service;
        public StockController(IStockDailyPriceService service)
        {
            _service = service;
        }
        // 5. 接收 HTML
        [HttpGet("")]
        public IActionResult Index()
        {
            return PhysicalFile(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Taiwan Stock Online.html"),
                "text/html");
        }

        // 6. 更新 DB
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UpdateStockRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.StockId))
                return BadRequest("StockId 不可為空");

            int endYear = DateTime.Now.Year;

            for (int year = req.StartYear; year <= endYear; year++)
            {
                await _service.FetchAndSaveAsync(year, req.StockId);
            }

            return Ok($"股票 {req.StockId} 已更新完成 ({req.StartYear} ~ {endYear})");
        }
        [HttpPost("update/single")]
        public async Task<IActionResult> UpdateSingle([FromBody] UpdateStockRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.StockId))
                return BadRequest("StockId 不可為空");

                await _service.FetchAndSaveAsync(req.StartYear, req.StockId);

            return Ok($"股票 {req.StockId} 已更新完成 ({req.StartYear})");
        }
        [HttpGet("{stockId}/daily")]
        public async Task<IActionResult> GetDailyPrices(string stockId)
        {
            var data = await _service.GetDailyPricesAsync(stockId);
            return Ok(data);
        }

    }
}
