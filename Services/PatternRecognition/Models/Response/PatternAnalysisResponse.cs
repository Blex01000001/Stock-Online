using Stock_Online.Services.KLine.Models.DTOs;
using Stock_Online.Services.PatternRecognition.Models.DTOs;

namespace Stock_Online.Services.PatternRecognition.Models.Response
{
    public class PatternAnalysisResponse
    {
        // 基礎 K 線資料，用於繪製底圖
        public KLineChartDto ChartData { get; set; }

        // 辨識出的型態清單，包含在 ChartData 中的索引位置
        public List<PatternMatchResult> DetectedPatterns { get; set; }
    }
}
