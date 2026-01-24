namespace Stock_Online.DataAccess
{
    public class StockUpdateJobLogger
    {
        private readonly FileLogger _log;

        public StockUpdateJobLogger(string logFile)
        {
            _log = new FileLogger(logFile);
        }

        // ===== 共用 =====
        public void JobStart(string title)
        {
            _log.Info("==================================================");
            _log.Info(title);
            _log.Info("==================================================");
        }
        public void JobEnd(TimeSpan totalTime)
        {
            _log.Info($"Total Time: {totalTime}");
            _log.Info("==================================================");
        }
        // ===== 單筆 =====
        public void SingleStockInfo(string stockId, int year)
        {
            _log.Info($"StockId : {stockId}");
            _log.Info($"Year    : {year}");
        }

        public void SingleStockSuccess(string stockId)
        {
            _log.Info($"Stock {stockId} update success");
        }

        public void SingleStockFail(string stockId, Exception ex)
        {
            _log.Error($"Stock {stockId} update failed");
            _log.Error(ex.Message);
        }
        // ===== 批次 =====
        public void JobStart(int year, int total)
        {
            _log.Info("==================================================");
            _log.Info("Stock Update Job Started");
            _log.Info($"Year      : {year}");
            _log.Info($"TotalStock: {total}");
            _log.Info("==================================================");
        }
        public void StockStart(string stockId, int index, int total)
        {
            _log.Info($"{stockId} Start ({index}/{total})");
        }
        public void StockSuccess(string stockId)
        {
            _log.Info($"{stockId} Success");
        }
        public void StockFail(string stockId, Exception? ex = null)
        {
            _log.Error($"{stockId} Failed");
            if (ex != null)
                _log.Error($"Error: {ex.Message}");
        }
        public void RetryFail(string stockId, int retry, Exception ex)
        {
            _log.Error($"Retry {retry}/3 failed for {stockId}: {ex.Message}");
        }
        public void JobSummary(
            int success,
            int fail,
            int total,
            IEnumerable<string> failedStocks,
            DateTime startTime)
        {
            var endTime = DateTime.Now;

            _log.Info("--------------------------------------------------");
            _log.Info("---------------------Summary----------------------");
            _log.Info("--------------------------------------------------");
            _log.Info($"Total   : {total}");
            _log.Info($"Success : {success}");
            _log.Info($"Failed  : {fail}");

            if (failedStocks.Any())
            {
                _log.Info("Failed Stock List:");
                foreach (var s in failedStocks)
                    _log.Info($"- {s}");
            }

            _log.Info($"Total Time: {endTime - startTime}");
            _log.Info($"EndTime  : {endTime}");
            _log.Info("==================================================");
        }
    }
}
