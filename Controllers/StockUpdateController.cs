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
        public StockUpdateController(IUpdateOrchestrator orchestrator)
        {
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

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteUpdate([FromBody] UpdateCommand command)
        {
            // 由於更新可能很久，通常業界會回傳一個 Job ID，而不是等待結果完成
            string jobId = await _orchestrator.QueueUpdateJobAsync(command);
            return Accepted(new { JobId = jobId });
        }
    }
}
