using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backtest
{
    public class Chart
    {
        public PlotModel Model { get; set; }

        private DateTimeAxis _xAxis;
        private LinearAxis _yAxis;
        private CandleStickSeries _candleSeries;

        public Chart(IEnumerable<OHLCV> ohlcvs, string symbol)
        {
            Model = new PlotModel
            {
                Background = OxyColor.FromRgb(25, 25, 25),
                TextColor = OxyColor.FromRgb(182, 182, 182),
                PlotAreaBorderColor = OxyColor.FromRgb(25, 25, 25)
            };

            CreateAxes();
            CreateSeries();
            LoadCandles(ohlcvs, symbol);
            ApplyInitialZoom(200);

            Model.Series.Add(_candleSeries);
            Model.InvalidatePlot(true);
        }

        private void CreateAxes()
        {
            _xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                MinorIntervalType = DateTimeIntervalType.Hours,
                IntervalType = DateTimeIntervalType.Days,
                IsZoomEnabled = true,
                IsPanEnabled = true,
                StringFormat = "dd"
            };

            _xAxis.AxisChanged += (s, e) =>
            {
                double totalDays = _xAxis.ActualMaximum - _xAxis.ActualMinimum;
                _xAxis.StringFormat = totalDays < 1 ? "HH:mm" : "dd";
            };

            _yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                IsZoomEnabled = true,
                IsPanEnabled = true
            };

            Model.Axes.Add(_xAxis);
            Model.Axes.Add(_yAxis);
        }

        private void CreateSeries()
        {
            _candleSeries = new CandleStickSeries
            {
                IncreasingColor = OxyColor.FromRgb(8, 153, 129),
                DecreasingColor = OxyColor.FromRgb(242, 54, 69)
            };
        }

        private void LoadCandles(IEnumerable<OHLCV> ohlcvs, string symbol)
        {
            foreach (var o in ohlcvs)
            {
                if (o.Symbol != symbol)
                    continue;

                _candleSeries.Items.Add(new HighLowItem(
                    DateTimeAxis.ToDouble(o.Hd.Timestamp),
                    o.High,
                    o.Low,
                    o.Open,
                    o.Close
                ));
            }
        }

        private void ApplyInitialZoom(int visibleCandles)
        {
            if (_candleSeries.Items.Count == 0)
                return;

            visibleCandles = Math.Min(visibleCandles, _candleSeries.Items.Count);

            var first = _candleSeries.Items[^visibleCandles];
            var last = _candleSeries.Items.Last();

            _xAxis.Minimum = first.X;
            _xAxis.Maximum = last.X;

            var lastCandles = _candleSeries.Items
                .TakeLast(visibleCandles)
                .ToList();

            double avgRange = lastCandles.Average(c => c.High - c.Low);
            double visibleRange = Math.Max(avgRange * 20, 0.0001);

            double lastClose = lastCandles.Last().Close;

            _yAxis.Minimum = lastClose - visibleRange / 2;
            _yAxis.Maximum = lastClose + visibleRange / 2;
        }


        public void UpdateData(IEnumerable<OHLCV> ohlcvs, string symbol)
        {
            // 1️ Vider les anciennes bougies
            _candleSeries.Items.Clear();

            // 2️ Ajouter les nouvelles bougies
            foreach (var o in ohlcvs.Where(x => x.Symbol == symbol))
            {
                _candleSeries.Items.Add(new HighLowItem(
                    DateTimeAxis.ToDouble(o.Hd.Timestamp),
                    o.High,
                    o.Low,
                    o.Open,
                    o.Close
                ));
            }

            // 3️ Recalculer le zoom initial
            ApplyInitialZoom(200);

            // 4️ Rafraichir le graphique
            Model.InvalidatePlot(true);
        }

    }
}
