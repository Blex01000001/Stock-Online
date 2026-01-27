using Microsoft.AspNetCore.Mvc;
using Stock_Online.Services.AnnualReview;

namespace Stock_Online.Controllers
{
    [ApiController]
    [Route("stock")]
    public class StockAnnualReviewController : ControllerBase
    {
        private readonly IStockAnnualReviewService _service;

        public StockAnnualReviewController(IStockAnnualReviewService service)
        {
            _service = service;
        }

        [HttpGet("annual-review")]
        public async Task<IActionResult> Get(string stockId)
        {

            if (string.IsNullOrWhiteSpace(stockId))
                return BadRequest("stockId required");

            var data = await _service.GetDataAsync(stockId);
            return Ok(data);
        }
    }
}
