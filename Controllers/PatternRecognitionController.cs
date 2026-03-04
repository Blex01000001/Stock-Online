using Microsoft.AspNetCore.Mvc;
using Stock_Online.Services.PatternRecognition.Models.DTOs;
using Stock_Online.Services.PatternRecognition.Models.Response;
using Stock_Online.Services.PatternRecognition;

namespace Stock_Online.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PatternRecognitionController : ControllerBase
    {
        private readonly IPatternRecognitionService _patternService;

        public PatternRecognitionController(IPatternRecognitionService patternService)
        {
            _patternService = patternService;
        }

        /// <summary>
        /// 取得單一股票的型態分析 (用於繪圖)
        /// </summary>
        [HttpGet("analyze/{stockId}")]
        public async Task<ActionResult<PatternAnalysisResponse>> GetAnalysis(
            string stockId,
            [FromQuery] string? start,
            [FromQuery] string? end)
        {

            var result = await _patternService.GetPatternAnalysisAsync(stockId, start, end);
            return Ok(result);
        }

        /// <summary>
        /// 掃描市場中符合特定型態的股票
        /// </summary>
        [HttpGet("scan/{patternName}")]
        public async Task<ActionResult<List<StockPatternSummary>>> ScanMarket(
            string patternName,
            [FromQuery] string? start,
            [FromQuery] string? end)
        {
            // 這裡暫時模擬獲取所有股票清單，實務上可從 Repo 取得
            //var allStockIds = new List<string> { "2330", "2317","2454", "2303", "9924", "1101", "1102", "1104", "1108", "1109", "1201", "1203", "1210", "1213", "1215" };
            var allStockIds = new List<string> { "1104", "1324", "1303" };
            //var allStockIds = new List<string> { "1104"};
            var results = await _patternService.ScanMarketPatternsAsync(allStockIds, patternName, start, end);
            return Ok(results);
        }
    }
}
