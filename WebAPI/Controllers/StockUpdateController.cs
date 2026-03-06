using Microsoft.AspNetCore.Mvc;
using Stock_Online.Services;
using Stock_Online.DTOs;
using Stock_Online.Common.Validation;
using Stock_Online.Application.Services.UpdateOrchestrator;
using Stock_Online.Application.DTOs.Commands;

namespace Stock_Online.WebAPI.Controllers
{
    [ApiController]
    [Route("stock")]
    public class StockUpdateController : ControllerBase
    {
        private readonly IUpdateOrchestrator _orchestrator;
        public StockUpdateController(IUpdateOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
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
