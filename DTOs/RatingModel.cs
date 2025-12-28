using Stock_Online.DataAccess.SQLite.Interface;

namespace Stock_Online.DTOs
{
    public class RatingModel
    {
        private readonly IStockDailyPriceRepository _repo;
        public DateTime TradeDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal NowPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinPrice { get; set; }
        public double RatingMax { get; set; }
        public double RatingMin { get; set; }
        public double RatingSub { get; set; }
        public void Ca()
        {
            RatingMax = Math.Round((double)(NowPrice / MaxPrice) * 100, 2);
            RatingMin = Math.Round((double)(NowPrice / MinPrice) * 100, 2);
            RatingSub = Math.Round(RatingMin - RatingMax, 0);
        }
    }
}
