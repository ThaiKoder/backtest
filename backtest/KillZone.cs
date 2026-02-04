using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace backtest
{

    public sealed class KillZone
    {
        public DateTime Start { get; }
        public DateTime End { get; }
        public long High { get; }
        public long Low { get; }

        public KillZone(DateTime start, DateTime end, long high, long low)
            => (Start, End, High, Low) = (start, end, high, low);

        public override string ToString()
            => $"KillZone: {Start:yyyy-MM-dd HH:mm:ss} -> {End:yyyy-MM-dd HH:mm:ss}, High={High}, Low={Low}";
    }




    public class KillZones
    {
        private readonly List<OHLCV> _candles;
        public List<KillZone> Zones { get; } = new List<KillZone>();
        private readonly PlotModel _model;

        private static readonly (string name, TimeSpan start, TimeSpan end)[] KillZoneHours =
        {
            ("Asia",   new(2, 0, 0),  new(6, 0, 0)),
            ("London", new(8, 0, 0),  new(11, 0, 0)),
            ("NY AM",  new(14, 30, 0), new(17, 0, 0))
        };

        public KillZones(PlotModel model, IEnumerable<OHLCV> candles)
        {
            _model = model;
            _candles = candles as List<OHLCV> ?? candles.ToList();
        }

        public static void AddKillZoneRectangle(
            PlotModel model,
            DateTime start,
            DateTime end,
            double high,
            double low,
            string label)
        {
            double x0 = DateTimeAxis.ToDouble(start);
            double x1 = DateTimeAxis.ToDouble(end);
            double xc = (x0 + x1) * 0.5;

            model.Annotations.Add(new RectangleAnnotation
            {
                MinimumX = x0,
                MaximumX = x1,
                MinimumY = low,
                MaximumY = high,
                Fill = OxyColor.FromAColor(150, OxyColors.White),
                Layer = AnnotationLayer.BelowSeries
            });

            model.Annotations.Add(new TextAnnotation
            {
                Text = label,
                TextPosition = new DataPoint(xc, low),
                TextHorizontalAlignment = HorizontalAlignment.Center,
                TextVerticalAlignment = VerticalAlignment.Top,
                Offset = new ScreenVector(0, 10),
                Stroke = OxyColors.Undefined,
                TextColor = OxyColors.White,
                FontSize = 11,
                Layer = AnnotationLayer.AboveSeries
            });
        }


        public KillZone CalculateZone(
            PlotModel model,
            DateTime start,
            DateTime end,
            string label = "")
        {
            long high = long.MinValue;
            long low = long.MaxValue;
            bool found = false;

            foreach (var c in _candles)
            {
                var t = c.Hd.Timestamp;
                if (t < start || t > end)
                    continue;

                found = true;

                if (c.High > high) high = c.High;
                if (c.Low < low) low = c.Low;
            }

            if (!found)
                throw new InvalidOperationException("Aucune bougie trouvée dans la plage.");

            var zone = new KillZone(start, end, high, low);
            return zone;
        }

        public void Show()
        {
            if (_candles == null || !_candles.Any())
                return;

            // Début et fin des bougies
            var firstCandle = _candles.First().Hd.Timestamp;
            var lastCandle = _candles.Last().Hd.Timestamp;

            // Boucles sur chaque jour
            var startDate = firstCandle.Date;
            var endDate = lastCandle.Date;

            Zones.Clear();

            for (var day = startDate; day <= endDate; day = day.AddDays(1))
            {
                foreach (var (name, from, to) in KillZoneHours)
                {
                    var zoneStart = day + from;
                    var zoneEnd = day + to;

                    // Vérifier que la zone chevauche la période des bougies
                    if (zoneEnd < firstCandle || zoneStart > lastCandle)
                        continue; // zone complètement en dehors → ignorer

                    // Ajuster la zone pour qu'elle reste dans les bougies existantes
                    var effectiveStart = zoneStart < firstCandle ? firstCandle : zoneStart;
                    var effectiveEnd = zoneEnd > lastCandle ? lastCandle : zoneEnd;

                    // Extraire les bougies de la zone une seule fois
                    var candlesInZone = _candles
                        .Where(c => c.Hd.Timestamp >= effectiveStart && c.Hd.Timestamp <= effectiveEnd)
                        .ToList();

                    if (!candlesInZone.Any())
                        continue;

                    // Calculer le high/low de la zone
                    long high = candlesInZone.Max(c => c.High);
                    long low = candlesInZone.Min(c => c.Low);

                    AddKillZoneRectangle(_model, effectiveStart, effectiveEnd, high, low, name);

                    var zone = new KillZone(effectiveStart, effectiveEnd, high, low);
                    Zones.Add(zone);

                    Debug.WriteLine($"{day:yyyy-MM-dd} {name}");
                    Debug.WriteLine($"KillZone: {effectiveStart:HH:mm}-{effectiveEnd:HH:mm}, High={high}, Low={low}");
                }
            }
        }
    }
}
