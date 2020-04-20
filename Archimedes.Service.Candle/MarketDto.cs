using System;
using System.Collections.Generic;
using Archimedes.Library.Types;

namespace Archimedes.Service.Candle
{
    public class MarketDto
    {
        public string Name { get; set; }
        public int Interval { get; set; }
        public GranularityType TimeFrame { get; set; }
        public bool Active { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public string TimeFrameInterval => $"{Interval}{TimeFrame.Value}";

        public override string ToString()
        {
            return $"Name: {Name} TimeFrameInterval: {TimeFrameInterval} Active: {Active} MaxDate: {MaxDate} LastUpdated: {LastUpdated}";
        }
    }
}