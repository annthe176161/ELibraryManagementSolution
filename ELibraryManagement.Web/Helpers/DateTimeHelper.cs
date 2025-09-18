using System;

namespace ELibraryManagement.Web.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        /// <summary>
        /// Lấy thời gian hiện tại theo múi giờ Việt Nam
        /// </summary>
        public static DateTime VietnamNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
        }

        /// <summary>
        /// Chuyển đổi DateTime UTC sang giờ Việt Nam
        /// </summary>
        public static DateTime ToVietnamTime(this DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
            {
                // Nếu không chỉ định, giả sử là UTC
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
            }
            else if (utcDateTime.Kind == DateTimeKind.Local)
            {
                // Nếu là Local time, chuyển sang UTC trước
                var utcTime = utcDateTime.ToUniversalTime();
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, VietnamTimeZone);
            }
            else
            {
                // Nếu đã là UTC
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
            }
        }

        /// <summary>
        /// Chuyển đổi DateTime sang UTC từ giờ Việt Nam
        /// </summary>
        public static DateTime FromVietnamTime(this DateTime vietnamDateTime)
        {
            if (vietnamDateTime.Kind == DateTimeKind.Unspecified)
            {
                // Nếu không chỉ định, giả sử là giờ Việt Nam
                return TimeZoneInfo.ConvertTimeToUtc(vietnamDateTime, VietnamTimeZone);
            }
            else if (vietnamDateTime.Kind == DateTimeKind.Local)
            {
                // Nếu là Local time, giả sử là giờ Việt Nam
                return TimeZoneInfo.ConvertTimeToUtc(vietnamDateTime, VietnamTimeZone);
            }
            else
            {
                // Nếu đã là UTC, trả về nguyên bản
                return vietnamDateTime;
            }
        }
    }
}