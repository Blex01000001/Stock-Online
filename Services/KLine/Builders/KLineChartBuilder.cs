using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using System.Diagnostics;

namespace Stock_Online.Services.KLine.Builders
{
    public class KLineChartBuilder
    {
        private readonly string _stockId;
        private readonly IReadOnlyList<StockDailyPrice> _prices;
        private readonly Dictionary<int, List<decimal?>> _maMap;
        private readonly int _index;
        private static readonly int[] MA_DAYS = { 5, 20, 60, 120, 240 };
        private readonly int _left;
        private readonly int _right;

        public KLineChartBuilder(string stockId, IReadOnlyList<StockDailyPrice> prices, int currDayIndex, Dictionary<int, List<decimal?>> maMap, bool area = true)
        {
            _stockId = stockId;
            _maMap = maMap;
            
            if (area)
            {
                // 以currDayIndex為中心，取前後50天
                _left = Math.Max(0, currDayIndex - 50);
                _right = Math.Min(prices.Count - 1, currDayIndex + 50);
            }
            else
            {
                _left = currDayIndex;
                _right = prices.Count - 1;
                Console.WriteLine($"left: {_left} right: {_right}");
            }

            _prices = prices
                .Skip(_left)
                .Take(_right - _left + 1)
                .ToList();

            _index = currDayIndex - _left;
        }

        public KLineChartDto Create()
        {
            var points = _prices.Select(x => new KLinePointDto
            {
                Date = x.TradeDate.ToString("yyyy-MM-dd"),
                Value = new[]
                {
                    x.OpenPrice,
                    x.ClosePrice,
                    x.LowPrice,
                    x.HighPrice
                },
                Volume = x.Volume
            }).ToList();

            //var closes = _prices.Select(x => x.ClosePrice).ToList();

            var maLines = MA_DAYS.Select(period =>
            {
                var fullMa = _maMap[period];

                return new MALineDto
                {
                    Name = $"MA{period}",
                    Values = fullMa
                        .Skip(_left)
                        .Take(_prices.Count)
                        .ToList()
                };
            }).ToList();

            var markLines = new List<KLineMarkLineDto>();

            int targetIndex = _index + 20;

            if (targetIndex < _prices.Count)
            {
                //markLines.Add(new KLineMarkLineDto
                //{
                //    Date = _prices[targetIndex].TradeDate.ToString("yyyy-MM-dd"),
                //    Type = "N+20",
                //    Label = "N+20"
                //});
            }
            return new KLineChartDto
            {
                StockId = _stockId,
                Points = points,
                MALines = maLines,
                Markers = new List<KLineMarkerDto> { new KLineMarkerDto
                {
                    Date = _prices[_index].TradeDate.ToString("yyyy-MM-dd"),
                    Type = "Selected",
                    Label = "N"
                }},
                MarkLines = markLines
            };

        }
        public KLineChartDto CreateSingle()
        {
            var points = _prices.Select(x => new KLinePointDto
            {
                Date = x.TradeDate.ToString("yyyy-MM-dd"),
                Value = new[]
                {
                    x.OpenPrice,
                    x.ClosePrice,
                    x.LowPrice,
                    x.HighPrice
                },
                Volume = x.Volume
            }).ToList();

            //var closes = _prices.Select(x => x.ClosePrice).ToList();

            List<MALineDto> maLines = MA_DAYS.Select(period =>
            {
                List<decimal?> fullMa = _maMap[period];

                return new MALineDto
                {
                    Name = $"MA{period}",
                    Values = fullMa
                        .Skip(_left)
                        .Take(_prices.Count)
                        .ToList()
                };
            }).ToList();

            //var markLines = new List<KLineMarkLineDto>();

            //int targetIndex = _index + 20;

            //if (targetIndex < _prices.Count)
            //{
            //    markLines.Add(new KLineMarkLineDto
            //    {
            //        Date = _prices[targetIndex].TradeDate.ToString("yyyy-MM-dd"),
            //        Type = "N+20",
            //        Label = "N+20"
            //    });
            //}
            return new KLineChartDto
            {
                StockId = _stockId,
                Points = points,
                MALines = maLines,
            };
        }
    }
}
