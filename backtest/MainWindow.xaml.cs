using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;


namespace backtest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PlotModel PlotModel { get; set; }

        public MainWindow()
        {
            ///////////////////////////////
            //READ
            ///////////////////////////////
            string filePath = "data.json";
            string jsonString = File.ReadAllText(filePath);


            ///////////////////////////////
            //Normalizer
            ///////////////////////////////
            var ohlcvs = OHLCVNormalizer.Normalize(filePath);


            ///////////////////////////////
            //Candle bus
            ///////////////////////////////

            ///////////////////////////////
            //Chart
            ///////////////////////////////

            InitializeComponent();
            DataContext = this;

            PlotModel = new PlotModel();

            PlotModel.Background = OxyColor.FromRgb(25, 25, 25);            // Fond noir
            PlotModel.TextColor = OxyColor.FromRgb(182, 182, 182);            // Texte général blanc
            PlotModel.PlotAreaBorderColor = OxyColor.FromRgb(25, 25, 25);   // Bordure du graphique


            // Axe X avec adaptation du format
            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                MinorIntervalType = DateTimeIntervalType.Hours,
                IntervalType = DateTimeIntervalType.Days,
                IsZoomEnabled = true,
                IsPanEnabled = true,
                StringFormat = "dd", // par défaut, juste le jour
            };

            // Hook pour ajuster dynamiquement le format selon l'échelle
            dateAxis.AxisChanged += (s, e) =>
            {
                var axis = s as DateTimeAxis;
                if (axis == null) return;

                double totalDays = axis.ActualMaximum - axis.ActualMinimum;

                if (totalDays < 1) // si les bougies sont très proches (moins d'un jour)
                    axis.StringFormat = "HH:mm"; // afficher l'heure
                else
                    axis.StringFormat = "dd"; // sinon juste le jour
            };

            PlotModel.Axes.Add(dateAxis);

            // Axe Y
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                IsZoomEnabled = true,
                IsPanEnabled = true
            };

            PlotModel.Axes.Add(yAxis);

            // Candlestick
            var candleSeries = new CandleStickSeries
            {
                IncreasingColor = OxyColor.FromRgb(8, 153, 129), // vert
                DecreasingColor = OxyColor.FromRgb(242, 54, 69) // rouge
            };


            foreach (var o in ohlcvs)
            {
                if (o.Symbol == "NQH6")

                {
                    candleSeries.Items.Add(new HighLowItem(
                        DateTimeAxis.ToDouble(o.Hd.Timestamp),
                        (long)o.High,
                        (long)o.Low,
                        (long)o.Open,
                        (long)o.Close
                    ));
                }

            }

            //PlotModel.Series.Add(candleSeries);

            // ZOOM INITIAL X (200 candles)
            int visibleCandles = Math.Min(200, candleSeries.Items.Count);

            var first = candleSeries.Items[^visibleCandles];
            var last = candleSeries.Items.Last();

            dateAxis.Minimum = first.X;
            dateAxis.Maximum = last.X;

            // ZOOM INITIAL Y
            var lastCandles = candleSeries.Items
                .TakeLast(visibleCandles)
                .ToList();

            double avgRange = lastCandles.Average(c => c.High - c.Low);
            double visibleRange = Math.Max(avgRange * 20, 0.0001);

            double lastClose = lastCandles.Last().Close;

            yAxis.Minimum = lastClose - visibleRange / 2;
            yAxis.Maximum = lastClose + visibleRange / 2;


            // REFRESH
            PlotModel.InvalidatePlot(true);
            PlotModel.Series.Add(candleSeries);
        }

        private void M5_Click(object sender, RoutedEventArgs e)
        {

        }

        private void backtest_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Backtest clicked");
        }
    }
}