using System;

namespace Tron.Wallet.Web {
    public static class DateTimeExtensions {
        public static long GetMillisecondTimestamp(this DateTime dateTime) {
            return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }

        public static long GetUnixTimestamp(this DateTime dateTime) {
            return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        public static DateTime GetDatetimeFromTimestamp(long timestamp) {
            return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(timestamp);
        }

        public static DateTime GetDatetimeFromMillisecondTimestamp(long timestamp) {
            return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddMilliseconds(timestamp);
        }
    }
}
