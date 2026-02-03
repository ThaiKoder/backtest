using System;
using System.Collections.Generic;
using System.Linq;

namespace backtest
{
    public class KillZone
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public long High { get; private set; }
        public long Low { get; private set; }

        public KillZone(DateTime start, DateTime end, long high, long low)
        {
            Start = start;
            End = end;
            High = high;
            Low = low;
        }

        public override string ToString()
        {
            return $"KillZone: {Start:yyyy-MM-dd HH:mm:ss} -> {End:yyyy-MM-dd HH:mm:ss}, High={High}, Low={Low}";
        }
    }

    public class KillZones
    {
        private readonly List<OHLCV> _candles;

        public KillZones(IEnumerable<OHLCV> candles)
        {
            _candles = candles.ToList();
        }

        /// <summary>
        /// Calcule la Kill Zone entre deux timestamps
        /// </summary>
        public KillZone CalculateZone(DateTime start, DateTime end)
        {
            var subset = _candles
                .Where(c => c.Hd.Timestamp >= start && c.Hd.Timestamp <= end)
                .ToList();

            if (!subset.Any())
                throw new Exception("Aucune bougie trouvée entre ces timestamps.");

            long high = subset.Max(c => c.High);
            long low = subset.Min(c => c.Low);

            return new KillZone(start, end, high, low);
        }
    }
}
