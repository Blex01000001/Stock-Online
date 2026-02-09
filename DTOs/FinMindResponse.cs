namespace Stock_Online.DTOs
{
    public sealed class FinMindResponse<T>
    {
        public string Msg { get; set; } = "";
        public int Status { get; set; }
        public List<T> Data { get; set; } = new();
    }
}
