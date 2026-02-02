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
    /// 

    public enum TimeFrames
    {
        M1,
        M5,
        M15,
        M30,
        H1,
        H4
    }
    public partial class MainWindow : Window
    {
        public Chart Chart { get; }
        string filePath = "data.json";
        string contractName = "NQH6";

        private CandleBus _candleBus = new CandleBus();
        private TimeFrames TimesFrameState = TimeFrames.M1;


        public MainWindow()
        {
            ///////////////////////////////
            //READ
            ///////////////////////////////
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
            Chart = new Chart(ohlcvs, contractName);
            DataContext = this;
        }

        private void M5_Click(object sender, RoutedEventArgs e)
        {
            if (TimesFrameState == TimeFrames.M5)
                return;


            var m1Data = OHLCVNormalizer.Normalize(filePath)
                .Where(o => o.Symbol == contractName)
                .ToList(); // ToList pour éviter plusieurs énumérations

            // Publier M5 (le CandleBus fera juste passer les données sans ré-agréger)
            _candleBus.Publish(m1Data, TimeFrame.M5);

            // Mettre à jour le chart
            Chart.UpdateData(_candleBus.CandleBusStream.ToList(), contractName);

            TimesFrameState = TimeFrames.M5;
        }


        private void backtest_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Backtest clicked");
        }

        private void M1_Click(object sender, RoutedEventArgs e)
        {
            if (TimesFrameState == TimeFrames.M1)
                return;


            var m1Data = OHLCVNormalizer.Normalize(filePath)
                .Where(o => o.Symbol == contractName)
                .ToList(); // ToList pour éviter plusieurs énumérations

            // Indiquer que c'est un clic utilisateur
            _candleBus.IsUserClick = true;


            // Publier M5 (le CandleBus fera juste passer les données sans ré-agréger)
            _candleBus.Publish(m1Data, TimeFrame.M1);

            // Mettre à jour le chart
            Chart.UpdateData(_candleBus.CandleBusStream.ToList(), contractName);

            TimesFrameState = TimeFrames.M1;

        }

        private void M15_Click(object sender, RoutedEventArgs e)
        {
            if (TimesFrameState == TimeFrames.M15)
                return;

            // Indiquer que c'est un clic utilisateur
            _candleBus.IsUserClick = true;

            var m1Data = OHLCVNormalizer.Normalize(filePath)
                .Where(o => o.Symbol == contractName)
                .ToList(); // ToList pour éviter plusieurs énumérations

            // Publier M5 (le CandleBus fera juste passer les données sans ré-agréger)
            _candleBus.Publish(m1Data, TimeFrame.M15);

            // Mettre à jour le chart
            Chart.UpdateData(_candleBus.CandleBusStream.ToList(), contractName);

            TimesFrameState = TimeFrames.M15;
        }

        private void M30_Click(object sender, RoutedEventArgs e)
        {

        }

        private void H1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void H4_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}