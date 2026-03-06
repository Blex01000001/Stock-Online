namespace Stock_Online.Application.DTOs.DataUpdater
{
    public class InstitutionalPointDto
    {
        public string Date { get; set; } = default!;
        public long Buy { get; set; }
        public long Sell { get; set; }
        public long Net => Buy - Sell;
    }
}
