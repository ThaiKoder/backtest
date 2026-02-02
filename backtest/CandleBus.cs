using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace backtest
{
    public enum TimeFrame { M1, M5, M15, M30, H1, H4 }

    public class CandleBus
    {
        public ObservableCollection<OHLCV> CandleBusStream { get; } = new ObservableCollection<OHLCV>();

        // Pour savoir si l'utilisateur a cliqué
        public bool IsUserClick { get; set; } = false;

        // Méthode publique pour publier des bougies
        public void Publish(IEnumerable<OHLCV> m1Data, TimeFrame tf)
        {
            if (IsUserClick)
            {
                // Renvoie le M1 correspondant
                foreach (var candle in m1Data)
                    CandleBusStream.Add(candle);
            }
            else
            {
                // Agréger selon le TimeFrame
                List<OHLCV> aggregated = tf switch
                {
                    TimeFrame.M1 => m1Data.ToList(),
                    TimeFrame.M5 => AggregateM1(m1Data, 5),
                    TimeFrame.M15 => AggregateM1(m1Data, 15),
                    TimeFrame.M30 => AggregateM1(m1Data, 30),
                    TimeFrame.H1 => AggregateM1(m1Data, 60),
                    TimeFrame.H4 => AggregateM1(m1Data, 240),
                    _ => m1Data.ToList()
                };

                foreach (var candle in aggregated)
                    CandleBusStream.Add(candle);
            }
        }

        private List<OHLCV> AggregateM1(IEnumerable<OHLCV> m1Data, int minutes)
        {
            var result = new List<OHLCV>();

            var groups = m1Data
                .OrderBy(c => c.Hd.Timestamp)
                .GroupBy(c => new DateTime(
                    c.Hd.Timestamp.Year,
                    c.Hd.Timestamp.Month,
                    c.Hd.Timestamp.Day,
                    c.Hd.Timestamp.Hour,
                    (c.Hd.Timestamp.Minute / minutes) * minutes,
                    0));

            foreach (var g in groups)
            {
                var first = g.First();
                var last = g.Last();

                result.Add(new OHLCV
                {
                    Hd = new Header
                    {
                        Timestamp = g.Key,
                        PublisherId = first.Hd.PublisherId,
                        InstrumentId = first.Hd.InstrumentId,
                        RType = first.Hd.RType
                    },
                    Open = first.Open,
                    High = g.Max(x => x.High),
                    Low = g.Min(x => x.Low),
                    Close = last.Close,
                    Volume = g.Sum(x => x.Volume),
                    Symbol = first.Symbol
                });
            }

            return result;
        }
    }
}
