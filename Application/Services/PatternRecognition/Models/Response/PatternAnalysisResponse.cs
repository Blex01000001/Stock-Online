using Stock_Online.Application.DTOs.KLine;
using Stock_Online.Application.DTOs.PatternRecognition;

namespace Stock_Online.Application.Services.PatternRecognition.Models.Response
{
    public class PatternAnalysisResponse
    {
        // 基礎 K 線資料，用於繪製底圖
        public KLineChartDto ChartData { get; set; }

        // 辨識出的型態清單，包含在 ChartData 中的索引位置
        public List<PatternMatchResult> DetectedPatterns { get; set; }
    }
}
