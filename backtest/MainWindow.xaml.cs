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
        public Chart Chart { get; }

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
            Chart = new Chart(ohlcvs, "NQH6");
            DataContext = this;
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