using Microsoft.AspNetCore.Mvc;
using Stock_Online.Services;
using Stock_Online.DTOs;
using Stock_Online.Services.Update;
using Stock_Online.Common.Validation;

namespace Stock_Online.Controllers
{
    [ApiController]
    [Route("stock")]
    public class StockUpdateController : ControllerBase
    {
        private readonly IStockPriceUpdateService _stockPriceUpdateService;
        public StockUpdateController(IStockPriceUpdateService stockPriceUpdateService)
        {
            this._stockPriceUpdateService = stockPriceUpdateService;
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
                await _stockPriceUpdateService.FetchAndSaveAsync(year, req.StockId);
            }

            return Ok($"股票 {req.StockId} 已更新完成 ({req.StartYear} ~ {endYear})");
        }
        [HttpPost("update/single")]
        public async Task<IActionResult> UpdateSingle([FromBody] UpdateStockRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.StockId))
                return BadRequest("StockId 不可為空");

                await _stockPriceUpdateService.FetchAndSaveAsync(req.StartYear, req.StockId);

            return Ok($"股票 {req.StockId} 已更新完成 ({req.StartYear})");
        }
        [HttpGet("{stockId}/daily")]
        public async Task<IActionResult> GetDailyPrices(string stockId)
        {
            var data = await _stockPriceUpdateService.GetDailyPricesAsync(stockId);
            return Ok(data);
        }
        [HttpPost("update/all-stock")]
        public async Task<IActionResult> UpdateAllStock([FromQuery] int year)
        {
            Console.WriteLine($"UpdateAllStock {year}");
            var (ok, error) = YearValidator.Validate(year, minYear: 2010);
            if (!ok)
                return BadRequest(error);

            _ = Task.Run(() => _stockPriceUpdateService.FetchAndSaveAllStockAsync(year));

            return Ok($"開始更新 {year} 年所有股票");
        }




    }
}
