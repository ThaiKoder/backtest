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
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                IsZoomEnabled = true,
                IsPanEnabled = true,
            });

            // Candlestick
            var candleSeries = new CandleStickSeries
            {
                IncreasingColor = OxyColor.FromRgb(8, 153, 129), // vert
                DecreasingColor = OxyColor.FromRgb(242, 54, 69) // rouge
            };

            // Remplir la série avec les OHLCV
            foreach (var o in ohlcvs)
            {
                double time = DateTimeAxis.ToDouble(o.Hd.Timestamp); // Conversion en double pour OxyPlot
                candleSeries.Items.Add(new HighLowItem(time, (double)o.High, (double)o.Low, (double)o.Open, (double)o.Close));
            }

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