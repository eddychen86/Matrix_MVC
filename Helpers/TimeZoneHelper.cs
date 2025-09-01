namespace Matrix.Helpers
{
    /// <summary>
    /// 時區處理輔助類
    /// 統一處理台北時區的時間轉換
    /// </summary>
    public static class TimeZoneHelper
    {
        /// <summary>
        /// 獲取台北時區的當前時間
        /// </summary>
        /// <returns>台北時區的當前時間</returns>
        public static DateTime GetTaipeiTime()
        {
            try
            {
                // Windows: "Taipei Standard Time", Linux: "Asia/Taipei"
                string timeZoneId = OperatingSystem.IsWindows() ? "Taipei Standard Time" : "Asia/Taipei";
                var taipeiTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, taipeiTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // 如果找不到時區，回退到 UTC+8
                return DateTime.UtcNow.AddHours(8);
            }
        }

        /// <summary>
        /// 獲取台北時區的今天開始時間（00:00:00）
        /// </summary>
        /// <returns>台北時區今天的開始時間</returns>
        public static DateTime GetTaipeiToday()
        {
            var taipeiNow = GetTaipeiTime();
            return new DateTime(taipeiNow.Year, taipeiNow.Month, taipeiNow.Day, 0, 0, 0);
        }

        /// <summary>
        /// 獲取台北時區指定天數前的時間
        /// </summary>
        /// <param name="days">天數</param>
        /// <returns>台北時區指定天數前的時間</returns>
        public static DateTime GetTaipeiTimeAddDays(int days)
        {
            return GetTaipeiTime().AddDays(days);
        }
    }
}