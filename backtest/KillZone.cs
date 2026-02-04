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
        private readonly PlotModel _model;

        private static readonly (string name, TimeSpan start, TimeSpan end)[] KillZoneHours =
        {
            ("Asia",   new(0, 0, 0),  new(3, 0, 0)),
            ("London", new(7, 0, 0),  new(10, 0, 0)),
            ("NY AM",  new(13, 0, 0), new(16, 0, 0))
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

            AddKillZoneRectangle(model, start, end, high, low, label);
            return new KillZone(start, end, high, low);
        }

        public void Show()
        {
            if (_candles == null || !_candles.Any())
                return;

            // Début et fin automatique à partir des timestamps
            var start = _candles.First().Hd.Timestamp;  // premier jour
            var end = _candles.Last().Hd.Timestamp;     // dernier jour

            for (var day = start; day <= end; day = day.AddDays(1))
            {
                foreach (var (name, from, to) in KillZoneHours)
                {
                    var zone = CalculateZone(
                        _model,
                        day.Add(from),
                        day.Add(to),
                        name
                    );

                    Debug.WriteLine($"{day:yyyy-MM-dd} {name}");
                    Debug.WriteLine(zone);
                }
            }
        }
    }
}
