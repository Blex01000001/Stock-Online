using Microsoft.AspNetCore.Mvc;
using Stock_Online.Domain.Enums;
using Stock_Online.DTOs;
using Stock_Online.Services.KLine;
using Stock_Online.Services.ROILine;

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
        [HttpGet("kline")]
        public async Task<IActionResult> GetKLine(
            [FromQuery] string stockId,
            [FromQuery] string? start,
            [FromQuery] string? end
        )
        {
            Console.WriteLine($"[KLine] {stockId} start={start} end={end}");

            if (string.IsNullOrWhiteSpace(stockId))
                return BadRequest("stockId is required");

            if (!string.IsNullOrWhiteSpace(start) && start.Length != 8)
                return BadRequest("start format must be yyyyMMdd");

            if (!string.IsNullOrWhiteSpace(end) && end.Length != 8)
                return BadRequest("end format must be yyyyMMdd");

            var result = await _kLineChartService.GetKLineAsync(
                stockId,
                0,
                start,
                end
            );
            Console.WriteLine($"result.Points.Count: {result.Count}");
            return Ok(result);
        }
        [HttpGet("kline/pattern/all-stock")]
        public async Task<IActionResult> GetPatternKLine([FromQuery] string pattern)
        {
            Console.WriteLine($"[GetPatternKLine] {pattern}");
            if (!CandlePatternParser.TryParse(pattern, out var candlePattern))
                return BadRequest($"Invalid pattern: {pattern}");

            string[] stocks = ["2327", "5880", "3034", "2883", "3008", "2002", "1303", "2603", "6919", "2059", "5871", "2207", "5876", "1301", "3045", "4904", "2395", "4938", "2912", "2615", "2609", "6505", "6446", "2618", "2376", "2449", "1504", "3036", "2356", "3044", "4958", "2324", "2105", "1102", "2347", "6239", "2385", "3005", "3702", "2377", "2353", "2474", "2027", "1477", "6176", "9904", "6285", "3023", "1319", "2006", "9941", "5347", "1402", "5434", "6257", "4766", "6196", "3030", "4915", "9939", "2610", "2206", "1210", "9917", "2393", "6278", "6670", "2504", "2441", "1215", "2607", "2850", "3010", "2015", "6605", "3592", "2404", "8299", "6121", "5522", "2606", "3211", "2637", "2451", "4763", "2371", "2458", "8112", "6005", "6757", "5508", "6412", "8070", "2439", "2520", "1442", "1326", "2409", "2633", "9945", "2352", "2845", "2915", "1722", "8454", "5483", "3260", "3264", "6147", "3227", "5388", "8016", "4961", "6197", "3617", "3078", "3090", "6206", "2801", "3037", "3665", "1101", "2834", "1590", "9910", "1476", "3706", "6472", "6789", "2838", "6409", "8210", "2889", "6592", "2211", "2201", "2204", "2855", "8926", "6414", "1609", "9933", "2467", "3042", "6214", "2362", "2515", "1707", "9930", "2103", "8131", "6806", "5515"];
            //string[] stocks = ["2330", "2327", "5880", "3034", "1303", "2603", "6919", "2059", "2207", "5876", "1301", "3045", "4904"];
            var result = new List<KLineChartDto>();

            foreach (var stock in stocks)
            {
                var charts = await _kLineChartService.GetKPatternLineAsync(
                    stock,
                    candlePattern
                );

                result.AddRange(charts);
            }

            Console.WriteLine($"result.Points.Count: {result.Count}");
            return Ok(result);
        }
        //[HttpGet("kline/pattern/stock")]
        //public async Task<IActionResult> GetPatternKLine(
        //    [FromQuery] string stockId,
        //    [FromQuery] int? days,
        //    [FromQuery] string? start,
        //    [FromQuery] string? end
        //)
        //{
        //    Console.WriteLine($"[MultipleLine] {stockId} days={days} start={start} end={end}");

        //    if (string.IsNullOrWhiteSpace(stockId))
        //        return BadRequest("stockId is required");

        //    if (days.HasValue && days <= 0)
        //        return BadRequest("days must be greater than 0");

        //    if (!string.IsNullOrWhiteSpace(start) && start.Length != 8)
        //        return BadRequest("start format must be yyyyMMdd");

        //    if (!string.IsNullOrWhiteSpace(end) && end.Length != 8)
        //        return BadRequest("end format must be yyyyMMdd");

        //    string[] stocks = ["2327", "5880", "3034", "2883", "3008", "2002", "1303", "2603", "6919", "2059", "5871", "2207", "5876", "1301", "3045", "4904", "2395", "4938", "2912", "2615", "2609", "6505", "6446", "2618", "2376", "2449", "1504", "3036", "2356", "3044", "4958", "2324", "2105", "1102", "2347", "6239", "2385", "3005", "3702", "2377", "2353", "2474", "2027", "1477", "6176", "9904", "6285", "3023", "1319", "2006", "9941", "5347", "1402", "5434", "6257", "4766", "6196", "3030", "4915", "9939", "2610", "2206", "1210", "9917", "2393", "6278", "6670", "2504", "2441", "1215", "2607", "2850", "3010", "2015", "6605", "3592", "2404", "8299", "6121", "5522", "2606", "3211", "2637", "2451", "4763", "2371", "2458", "8112", "6005", "6757", "5508", "6412", "8070", "2439", "2520", "1442", "1326", "2409", "2633", "9945", "2352", "2845", "2915", "1722", "8454", "5483", "3260", "3264", "6147", "3227", "5388", "8016", "4961", "6197", "3617", "3078", "3090", "6206", "2801", "3037", "3665", "1101", "2834", "1590", "9910", "1476", "3706", "6472", "6789", "2838", "6409", "8210", "2889", "6592", "2211", "2201", "2204", "2855", "8926", "6414", "1609", "9933", "2467", "3042", "6214", "2362", "2515", "1707", "9930", "2103", "8131", "6806", "5515"];
        //    //string[] stocks = ["2330"];
        //    var result = new List<KLineChartDto>();

        //    foreach (var stock in stocks)
        //    {
        //        var charts = await _kLineChartService.GetKPatternLineAsync(
        //            stock,
        //            CandlePattern.FlatBottom
        //        );

        //        result.AddRange(charts);
        //    }

        //    Console.WriteLine($"result.Points.Count: {result.Count}");
        //    return Ok(result);
        //}

    }
}
