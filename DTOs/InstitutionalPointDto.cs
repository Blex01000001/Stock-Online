namespace Stock_Online.DTOs
{
    public class InstitutionalPointDto
    {
        public string Date { get; set; } = default!;
        public long Buy { get; set; }
        public long Sell { get; set; }
        public long Net => Buy - Sell;
    }
}
