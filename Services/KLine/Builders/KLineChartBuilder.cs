using Stock_Online.Domain.Entities;
using Stock_Online.Services.DataUpdater.Models.DTOs;
using Stock_Online.Services.KLine.Indicators;
using Stock_Online.Services.KLine.Models.DTOs;

namespace Stock_Online.Services.KLine.Builders
{
    public class KLineChartBuilder
    {
        private readonly string _stockId;
        private readonly string _stockName;
        private readonly int _left;
        private readonly int _right;
        private DateTime _date;
        private readonly IReadOnlyList<StockDailyPrice> _prices;
        private readonly IReadOnlyList<StockDailyPrice> _allOriginalPrices;
        private List<StockShareholding> _shareholdings;
        private InstitutionalSeriesDto _institutionalSeriesDto = new InstitutionalSeriesDto();
        private MacdDto _macd;
        private BollingerBandsDto _bollingerBands;
        private List<KLinePointDto> _points;
        private List<MALineDto> _maLines;
        private RsiDto _rsi;

        public KLineChartBuilder(string stockId, string stockName, IReadOnlyList<StockDailyPrice> prices, int startIndex)
        {
            _stockId = stockId;
            _stockName = stockName;
            _allOriginalPrices = prices;
            _left = startIndex;
            _right = prices.Count - 1;
            _prices = prices.Skip(_left).ToList();
            _date = prices[startIndex].TradeDate;

            // 初始化空容器，僅在需要時填充
            _institutionalSeriesDto = new InstitutionalSeriesDto();
            _shareholdings = new List<StockShareholding>();
        }
        public KLineChartDto Build()
        {
            SetPoints();

            return new KLineChartDto
            {
                StockId = _stockId,
                StockName = _stockName,
                Points = _points,
                MALines = _maLines,
                Shareholdings = _shareholdings,
                Institutional = _institutionalSeriesDto,
                Macd = _macd,
                BollingerBands = _bollingerBands,
                Rsi = _rsi
            };
        }
        private void SetPoints()
        {
            _points = _prices.Select(x => new KLinePointDto
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
        }
        public KLineChartBuilder SetRsi(int period = 14)
        {
            var Rsi66 = Indicator.CalculateRsi(_allOriginalPrices, 6).Skip(_left).Take(_right - _left + 1);
            var Rsi1212 = Indicator.CalculateRsi(_allOriginalPrices, 6).Skip(_left).Take(_right - _left + 1);
            var Rsi2424 = Indicator.CalculateRsi(_allOriginalPrices, 6).Skip(_left).Take(_right - _left + 1);

            _rsi = new RsiDto()
            {
                Rsi6 = Indicator.CalculateRsi(_allOriginalPrices, 6).Skip(_left).Take(_right - _left + 1).ToList(),
                Rsi12 = Indicator.CalculateRsi(_allOriginalPrices, 12).Skip(_left).Take(_right - _left + 1).ToList(),
                Rsi24 = Indicator.CalculateRsi(_allOriginalPrices, 24).Skip(_left).Take(_right - _left + 1).ToList()
            };
            return this;
        }
        public KLineChartBuilder SetMacd(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            var fullMacd = Indicator.CalculateMacd(_allOriginalPrices, fastPeriod, slowPeriod, signalPeriod);
            _macd = new MacdDto
            {
                Dif = fullMacd.Dif.Skip(_left).Take(_right - _left + 1).ToList(),
                Dea = fullMacd.Dea.Skip(_left).Take(_right - _left + 1).ToList(),
                Hist = fullMacd.Hist.Skip(_left).Take(_right - _left + 1).ToList()
            };
            return this;
        }
        public KLineChartBuilder SetBollingerBand(int maDay = 20, int StandardDeviation = 2)
        {
            var fullBB = Indicator.CalculateBollingerBands(_allOriginalPrices, maDay, StandardDeviation);
            _bollingerBands = new BollingerBandsDto
            {
                Mid = fullBB.Mid.Skip(_left).Take(_right - _left + 1).ToList(),
                Upper = fullBB.Upper.Skip(_left).Take(_right - _left + 1).ToList(),
                Lower = fullBB.Lower.Skip(_left).Take(_right - _left + 1).ToList()
            };
            return this;
        }
        public KLineChartBuilder SetMaLine(int[] MaDays = null)
        {
            MaDays ??= new int[] { 5, 20, 60, 120, 240 };
            _maLines = MaDays.Select(period =>
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
            return this;
        }
        public KLineChartBuilder SetForeignInvestmentHolding(List<StockShareholding> stockShareholdings)
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

            // 因外資持股的資料會包含沒有交易的天數，所以資料天數會比price還要多
            //_shareholdings = stockShareholdings.Skip(index).Take(_right - _left + 1).ToList();
            _shareholdings = stockShareholdings.Skip(index).ToList();

            return this;
        }
        public KLineChartBuilder SetInstitutional(List<StockInstitutionalInvestorsBuySell> institutionalInvestorsBuySell)
        {
            if (institutionalInvestorsBuySell == null || institutionalInvestorsBuySell.Count == 0) return this;
            string targetDate = _date.ToString("yyyy-MM-dd");

            var matched = institutionalInvestorsBuySell
                .Where(x => x.Date.CompareTo(targetDate) <= 0)
                .OrderByDescending(x => x.Date)
                .FirstOrDefault();

            int index = institutionalInvestorsBuySell.IndexOf(matched) / 5;

            _institutionalSeriesDto.Daily = institutionalInvestorsBuySell
                .GroupBy(x => x.Date)
                .Select(g =>
                {
                    var lookup = g.ToLookup(x => x.Name);
                    long GetNet(string name) => lookup[name].Sum(x => x.Buy - x.Sell);

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
                .Skip(index).Take(_right - _left + 1)
                .ToList();

            return this;
        }
    }
}
