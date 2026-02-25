using Stock_Online.Domain.Entities;
using Stock_Online.DTOs;
using Stock_Online.Services.KLine.Indicators;

namespace Stock_Online.Services.KLine.Builders
{
    public class KLineChartBuilder
    {
        private readonly string _stockId;
        private readonly IReadOnlyList<StockDailyPrice> _prices;
        private readonly IReadOnlyList<StockDailyPrice> _allOriginalPrices;
        private readonly Dictionary<int, List<decimal?>> _maMap;
        private readonly int _index;
        private static readonly int[] MA_DAYS = { 5, 20, 60, 120, 240 };
        private readonly int _left;
        private readonly int _right;
        private DateTime _date;
        private List<StockShareholding> _shareholdings;
        private InstitutionalSeriesDto _institutionalSeriesDto = new InstitutionalSeriesDto();
        private MacdDto _fullMacd;

        public KLineChartBuilder(string stockId, IReadOnlyList<StockDailyPrice> prices, int currDayIndex)
        {
            _stockId = stockId;
            _fullMacd = Indicator.CalculateMacd(prices);
            _left = currDayIndex;
            _right = prices.Count - 1;
            _allOriginalPrices = prices;
            _prices = prices
                .Skip(_left)
                .Take(_right - _left + 1)
                .ToList();

            _index = currDayIndex - _left;
            _date = prices[currDayIndex].TradeDate;
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

            List<MALineDto> maLines = MA_DAYS.Select(period =>
            {
                List<decimal?> fullMa = Indicator.CalculateSma(_allOriginalPrices, period);

                return new MALineDto
                {
                    Name = $"MA{period}",
                    Values = fullMa
                        .Skip(_left)
                        .Take(_prices.Count)
                        .ToList()
                };
            }).ToList();

            var fullBB = Indicator.CalculateBollingerBands(_allOriginalPrices, 20, 2);

            return new KLineChartDto
            {
                StockId = _stockId,
                Points = points,
                MALines = maLines,
                Shareholdings = _shareholdings,
                Institutional = _institutionalSeriesDto,
                Macd = new MacdDto
                {
                    Dif = _fullMacd.Dif.Skip(_left).Take(_right - _left + 1).ToList(),
                    Dea = _fullMacd.Dea.Skip(_left).Take(_right - _left + 1).ToList(),
                    Hist = _fullMacd.Hist.Skip(_left).Take(_right - _left + 1).ToList()
                },
                BollingerBands = new BollingerBandsDto
                {
                    Mid = fullBB.Mid.Skip(_left).Take(_right - _left + 1).ToList(),
                    Upper = fullBB.Upper.Skip(_left).Take(_right - _left + 1).ToList(),
                    Lower = fullBB.Lower.Skip(_left).Take(_right - _left + 1).ToList()
                }
            };
        }
        public KLineChartBuilder SetHolding(List<StockShareholding> stockShareholdings)
        {
            if (stockShareholdings == null || stockShareholdings.Count == 0)
            {
                _shareholdings = new List<StockShareholding>();
                return this;
            }

            string targetDate = _date.ToString("yyyy-MM-dd");

            var matched = stockShareholdings
                .Where(x => x.Date.CompareTo(targetDate) <= 0)
                .OrderByDescending(x => x.Date)
                .FirstOrDefault();

            if (matched == null)
            {
                _shareholdings = new List<StockShareholding>();
                return this;
            }

            int index = stockShareholdings.IndexOf(matched);
            _shareholdings = stockShareholdings.Skip(index).ToList();

            return this;
        }
        public KLineChartBuilder SetInstitutional(List<StockInstitutionalInvestorsBuySell> institutionalInvestorsBuySell)
        {
            if (institutionalInvestorsBuySell == null || institutionalInvestorsBuySell.Count == 0)
            {
                return this;
            }

            string targetDate = _date.ToString("yyyy-MM-dd");

            var matched = institutionalInvestorsBuySell
                .Where(x => x.Date.CompareTo(targetDate) <= 0)
                .OrderByDescending(x => x.Date)
                .FirstOrDefault();

            int index = institutionalInvestorsBuySell.IndexOf(matched);


            _institutionalSeriesDto.Daily = institutionalInvestorsBuySell
                .GroupBy(x => x.Date)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    long GetNet(string name) =>
                        g.Where(x => x.Name == name).Sum(x => x.Buy - x.Sell);

                    var foreignInvestor = GetNet("Foreign_Investor");
                    var foreignDealerSelf = GetNet("Foreign_Dealer_Self");
                    var dealerSelf = GetNet("Dealer_self");
                    var dealerHedging = GetNet("Dealer_Hedging");
                    var investTrust = GetNet("Investment_Trust");

                    return new InstitutionalDailyDto
                    {
                        Date = g.Key,

                        ForeignInvestorNet = foreignInvestor,
                        ForeignDealerSelfNet = foreignDealerSelf,
                        DealerSelfNet = dealerSelf,
                        DealerHedgingNet = dealerHedging,

                        ForeignNet = foreignInvestor + foreignDealerSelf,
                        DealerNet = dealerSelf + dealerHedging,
                        InvestmentTrustNet = investTrust
                    };
                })
                .ToList();

            return this;
        }
    }
}
