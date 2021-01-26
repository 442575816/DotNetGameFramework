using System;

namespace DotNetGameFramework
{
    /// <summary>
    /// 工具类
    /// </summary>
    public static class TimeUtil
    {
        /// <summary>
        /// 1970时间开始的DateTime
        /// </summary>
        private readonly static DateTime DT_1970 = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// 1970时间开始的Tick
        /// </summary>
        private readonly static long DT_1970_TICKS = DT_1970.Ticks;

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        public static long Timestamp => new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

        /// <summary>
        /// 获取时间间隔，与当前时间做比较
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetTimeSpan(long time)
        {
            return Timestamp - time;
        }

        /// <summary>
        /// 判断是否是同一天
        /// </summary>
        /// <param name="createTime"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public static bool IsSameDay(long time1, DateTime time2)
        {
            return IsSameDay(GetDateTime(time1), time2);
        }

        /// <summary>
        /// 判断是否是同一天
        /// </summary>
        /// <param name="createTime"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public static bool IsSameDay(long time1, long time2)
        {
            return IsSameDay(GetDateTime(time1), GetDateTime(time2));
        }

        /// <summary>
        /// 判断是否是同一天
        /// </summary>
        /// <param name="createTime"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public static bool IsSameDay(DateTime time1, DateTime time2)
        {
            return time1.DayOfYear == time2.DayOfYear && time1.Year == time2.Year;
        }

        /// <summary>
        /// 时间戳转换为时间
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(long timestamp)
        {
            long timeTicks = timestamp * 10000 + DT_1970_TICKS;

            return new DateTime(timeTicks).ToLocalTime();
        }
    }
}
