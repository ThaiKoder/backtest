using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

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

        public static void AddKillZoneRectangle(
            PlotModel model,
            DateTime Start,
            DateTime End,
            double High,
            double Low,
            string labelText)
        {
            var xStart = DateTimeAxis.ToDouble(Start);
            var xEnd = DateTimeAxis.ToDouble(End);
            var xCenter = (xStart + xEnd) / 2.0;

            // Rectangle
            var rectangle = new RectangleAnnotation
            {
                MinimumX = xStart,
                MaximumX = xEnd,
                MinimumY = Low,
                MaximumY = High,

                Fill = OxyColor.FromAColor(150, OxyColors.White),
                Layer = AnnotationLayer.BelowSeries
            };

            model.Annotations.Add(rectangle);

            // Label sous le rectangle
            var label = new TextAnnotation
            {
                Text = labelText,

                // Position en coordonnées DATA
                TextPosition = new DataPoint(xCenter, Low),

                // Alignement
                TextHorizontalAlignment = HorizontalAlignment.Center,
                TextVerticalAlignment = VerticalAlignment.Top,

                // Décalage en PIXELS (vers le bas)
                Offset = new ScreenVector(0, 10),

                Stroke = OxyColors.Undefined,
                TextColor = OxyColors.White,
                FontSize = 11,

                Layer = AnnotationLayer.AboveSeries
            };

            model.Annotations.Add(label);
        }

        public KillZones(IEnumerable<OHLCV> candles)
        {
            _candles = candles.ToList();
        }

        /// <summary>
        /// Calcule la Kill Zone entre deux timestamps
        /// </summary>
        public KillZone CalculateZone(PlotModel model, DateTime start, DateTime end, string label = "")
        {
            var subset = _candles
                .Where(c => c.Hd.Timestamp >= start && c.Hd.Timestamp <= end)
                .ToList();

            if (!subset.Any())
                throw new Exception("Aucune bougie trouvée entre ces timestamps.");

            long high = subset.Max(c => c.High);
            long low = subset.Min(c => c.Low);

            AddKillZoneRectangle(model, start, end, high, low, label);
            return new KillZone(start, end, high, low);
        }
    }
}
