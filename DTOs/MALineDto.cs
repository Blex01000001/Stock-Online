namespace Stock_Online.DTOs
{
    public class MALineDto
    {
        /// <summary>
        /// MA 名稱：MA5 / MA20 / MA60 / MA120 / MA240
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 與 K 線日期對齊，沒有值用 null
        /// </summary>
        public List<decimal?> Values { get; set; } = new();
    }
}
