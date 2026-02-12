using Microsoft.AspNetCore.Mvc;
using Stock_Online.Services;
using Stock_Online.DTOs;
using Stock_Online.Services.Update;
using Stock_Online.Common.Validation;
using Stock_Online.DTOs.UpdateRequest;
using Stock_Online.Services.UpdateOrchestrator;

namespace Stock_Online.Controllers
{
    [ApiController]
    [Route("stock")]
    public class StockUpdateController : ControllerBase
    {
        private readonly IUpdateOrchestrator _orchestrator;
        //private readonly IStockPriceUpdateService _priceUpdateService;
        //private readonly IStockDividendUpdateService _dividendUpdateService;
        //private readonly IStockShareholdingUpdateService _shareholdingUpdateService;
        public StockUpdateController(
            //IStockPriceUpdateService priceUpdateService,
            //IStockDividendUpdateService dividendUpdateService,
            //IStockShareholdingUpdateService shareholdingUpdateService,
            IUpdateOrchestrator orchestrator
            )
        {
            //this._priceUpdateService = priceUpdateService;
            //this._dividendUpdateService = dividendUpdateService;
            //this._shareholdingUpdateService = shareholdingUpdateService;
            this._orchestrator = orchestrator;
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
        //[HttpPost("update")]
        //public async Task<IActionResult> Update([FromBody] UpdateStockRequest req)
        //{
        //    if (string.IsNullOrWhiteSpace(req.StockId))
        //        return BadRequest("StockId 不可為空");

        //    int endYear = DateTime.Now.Year;

        //    for (int year = req.StartYear; year <= endYear; year++)
        //    {
        //        await _priceUpdateService.FetchAndSaveAsync(year, req.StockId);
        //    }

        //    return Ok($"股票 {req.StockId} 已更新完成 ({req.StartYear} ~ {endYear})");
        //}


        // DailyPrice
        //[HttpGet("{stockId}/daily")]
        //public async Task<IActionResult> GetDailyPrices(string stockId)
        //{
        //    var data = await _priceUpdateService.GetDailyPricesAsync(stockId);
        //    return Ok(data);
        //}
        //[HttpPost("update/price/single-stock")]
        //public async Task<IActionResult> UpdateSingle([FromBody] UpdateStockRequest req)
        //{
        //    if (string.IsNullOrWhiteSpace(req.StockId))
        //        return BadRequest("StockId 不可為空");

        //        await _priceUpdateService.FetchAndSaveAsync(req.StartYear, req.StockId);

        //    return Ok($"股票 {req.StockId} 已更新完成 ({req.StartYear})");
        //}
        //[HttpPost("update/price/all-stock")]
        //public async Task<IActionResult> UpdateAllStock([FromQuery] int year)
        //{
        //    Console.WriteLine($"UpdateAllStock {year}");
        //    var (ok, error) = YearValidator.Validate(year, minYear: 2010);
        //    if (!ok)
        //        return BadRequest(error);

        //    _ = Task.Run(() => _priceUpdateService.FetchAndSaveAllStockAsync(year));

        //    return Ok($"開始更新 {year} 年所有股票");
        //}


        // Shareholding
        //[HttpPost("update/Shareholding/single-stock")]
        //public async Task<IActionResult> UpdateShareholding([FromQuery] string stockId)
        //{
        //    Console.WriteLine($"Update Share Holding");
        //    _ = Task.Run(() => _shareholdingUpdateService.FetchAndSaveAsync(stockId));
        //    return Ok($"Start Share Holding");
        //}


        // Dividend
        //[HttpPost("update/dividend/all-stock")]
        //public async Task<IActionResult> UpdateAll()
        //{
        //    Console.WriteLine($"Update All Dividend");
        //    _ = Task.Run(() => _dividendUpdateService.FetchAndSaveAllStockAsync());
        //    return Ok($"Start 所有股票股利資訊");
        //}


        // ExecuteUpdate
        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteUpdate([FromBody] UpdateCommand command)
        {
            // 由於更新可能很久，通常業界會回傳一個 Job ID，而不是等待結果完成
            string jobId = await _orchestrator.QueueUpdateJobAsync(command);
            return Accepted(new { JobId = jobId });
        }
    }
}
