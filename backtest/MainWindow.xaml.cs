using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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