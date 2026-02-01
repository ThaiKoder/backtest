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
            // Liste pour stocker les OHLCV
            List<OHLCV> ohlcvs = new List<OHLCV>();

            // Lire chaque ligne du fichier
            foreach (var line in File.ReadLines(filePath))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    try
                    {
                        OHLCV? ohlcv = JsonSerializer.Deserialize<OHLCV>(line);
                        if (ohlcv != null)
                        {
                            ohlcvs.Add(ohlcv);
                        }
                    }
                    catch (JsonException ex)
                    {
                        Debug.WriteLine($"Erreur de désérialisation : {ex.Message}");
                    }
                }
            }

            //// Exemple d'affichage
            foreach (var o in ohlcvs)
            {
                Debug.WriteLine($"{o.Symbol} - {o.Hd.Timestamp}: O={o.Open}, H={o.High}, L={o.Low}, C={o.Close}, V={o.Volume}");
            }


            ///////////////////////////////
            //Candle bus
            ///////////////////////////////

            ///////////////////////////////
            //Chart
            ///////////////////////////////

            InitializeComponent();
            DataContext = this;

            PlotModel = new PlotModel { Title = "Single Candlestick" };

            // Axe X
            PlotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "HH:mm",
                IsZoomEnabled = true,
                IsPanEnabled = true
            });

            // Axe Y
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                IsZoomEnabled = true,
                IsPanEnabled = true
            });

            // Candlestick
            var candleSeries = new CandleStickSeries
            {
                IncreasingColor = OxyColors.Green,
                DecreasingColor = OxyColors.Red
            };

            double open = 100;
            double high = 105;
            double low = 95;
            double close = 102;
            double time = DateTimeAxis.ToDouble(DateTime.Now);

            candleSeries.Items.Add(new HighLowItem(time, high, low, open, close));
            PlotModel.Series.Add(candleSeries);
        }
    }
}