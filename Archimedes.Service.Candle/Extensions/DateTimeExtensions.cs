using System;

namespace Archimedes.Service.Candle
{
    public static class DateTimeExtensions
    {
        //todo need to switch between hours and minutes
        public static DateTime RoundDownTime(this DateTime dateTime, int interval)
        {
            var intervalSpan = TimeSpan.FromMinutes(interval);
            var result = dateTime.AddMinutes(-interval);

            return result.Floor(intervalSpan);
        }

        public static DateTime Floor(this DateTime dateTime, TimeSpan interval)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % interval.Ticks));
        }

        public static DateTime Ceiling(this DateTime dateTime, TimeSpan interval)
        {
            var overflow = dateTime.Ticks % interval.Ticks;

            return overflow == 0 ? dateTime : dateTime.AddTicks(interval.Ticks - overflow);
        }

        public static DateTime Round(this DateTime dateTime, TimeSpan interval)
        {
            var halfIntervalTicks = (interval.Ticks + 1) >> 1;

            return dateTime.AddTicks(halfIntervalTicks - ((dateTime.Ticks + halfIntervalTicks) % interval.Ticks));
        }
    }
}