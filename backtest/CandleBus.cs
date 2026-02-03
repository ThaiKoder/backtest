using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace backtest
{
    public enum TimeFrame { M1, M5, M15, M30, H1, H4 }
    public class CandleBus
    {
        public IEnumerable<OHLCV> BuildFromM1(
            IEnumerable<OHLCV> m1Data,
            TimeFrame targetTf)
        {
            if (targetTf == TimeFrame.M1)
                return m1Data;

            int minutes = targetTf switch
            {
                TimeFrame.M5 => 5,
                TimeFrame.M15 => 15,
                TimeFrame.M30 => 30,
                TimeFrame.H1 => 60,
                TimeFrame.H4 => 240,
                _ => throw new ArgumentOutOfRangeException()
            };

            return m1Data
    .GroupBy(c =>
    {
        var ts = c.Hd.Timestamp;
        return new DateTime(
            ts.Year,
            ts.Month,
            ts.Day,
            ts.Hour,
            (ts.Minute / minutes) * minutes,
            0,
            DateTimeKind.Utc
        );
    })
    .Select(g =>
    {
        var ordered = g.OrderBy(x => x.Hd.Timestamp).ToList();

        return new OHLCV
        {
            Hd = new Header
            {
                Timestamp = g.Key,
                InstrumentId = ordered.First().Hd.InstrumentId,
                PublisherId = ordered.First().Hd.PublisherId,
                RType = ordered.First().Hd.RType
            },
            Open = ordered.First().Open,
            High = ordered.Max(x => x.High),
            Low = ordered.Min(x => x.Low),
            Close = ordered.Last().Close,
            Volume = ordered.Sum(x => x.Volume),
            Symbol = ordered.First().Symbol
        };
    })
    .OrderBy(x => x.Hd.Timestamp)
    .ToList();
        }
    }
}