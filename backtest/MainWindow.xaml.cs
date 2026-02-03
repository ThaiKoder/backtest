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

    public enum SessionType
    {
        RTH,
        ETH
    }

    public partial class MainWindow : Window
    {
        public Chart Chart { get; }
        string filePath = "data.json";
        string contractName = "NQH6";

        private CandleBus _candleBus = new CandleBus();
        private TimeFrames TimesFrameState = TimeFrames.M1;
        private SessionType SessionTypeState = SessionType.RTH;
        private List<OHLCV> m1Data;
        private List<OHLCV> timeFrameData;


        public MainWindow()
        {
            ///////////////////////////////
            //READ
            ///////////////////////////////
            string jsonString = File.ReadAllText(filePath);


            ///////////////////////////////
            //Normalizer
            ///////////////////////////////
            m1Data = OHLCVNormalizer.Normalize(filePath)
                .Where(o => o.Symbol == contractName)
                .ToList();

            timeFrameData = m1Data;


            ///////////////////////////////
            //Candle bus
            ///////////////////////////////



            ///////////////////////////////
            //Chart
            ///////////////////////////////
            Chart = new Chart(m1Data, contractName);
            DataContext = this;
        }


        private bool IsRTH(DateTime t) //UTC time
        {
            if (t.DayOfWeek == DayOfWeek.Saturday || t.DayOfWeek == DayOfWeek.Sunday)
                return false;

            var time = t.TimeOfDay;
            return time >= new TimeSpan(14, 30, 0)
                && time <= new TimeSpan(21, 0, 0);
        }


        private bool IsETH(DateTime t) // UTC time
        {
            if (t.DayOfWeek == DayOfWeek.Saturday || t.DayOfWeek == DayOfWeek.Sunday)
                return false;

            var time = t.TimeOfDay;

            // session qui traverse minuit
            return time >= new TimeSpan(23, 0, 0)
                || time < new TimeSpan(22, 0, 0);
        }



        private PanelWindow _panel;

        private void OpenPanel()
        {
            if (_panel != null)
            {
                _panel.Activate();
                return;
            }

            _panel = new PanelWindow
            {
                Owner = this // 🔗 lien minimal
            };

            _panel.Closed += (s, e) => _panel = null;
            _panel.Show();
        }



        private void backtest_Click(object sender, RoutedEventArgs e)
        {
            OpenPanel();
            Debug.WriteLine("Backtest clicked");
        }

        private void M1_Click(object sender, RoutedEventArgs e)
        {
            if (TimesFrameState == TimeFrames.M1)
                return;

            var busData = _candleBus.BuildFromM1(timeFrameData, TimeFrame.M1);
            Chart.UpdateData(busData, contractName);

            TimesFrameState = TimeFrames.M1;
        }

        private void M5_Click(object sender, RoutedEventArgs e)
        {
            if (TimesFrameState == TimeFrames.M5)
                return;

            var busData = _candleBus.BuildFromM1(timeFrameData, TimeFrame.M5);
            Chart.UpdateData(busData, contractName);

            TimesFrameState = TimeFrames.M5;
        }


        private void M15_Click(object sender, RoutedEventArgs e)
        {
            if (TimesFrameState == TimeFrames.M15)
                return;

            var busData = _candleBus.BuildFromM1(timeFrameData, TimeFrame.M15);
            Chart.UpdateData(busData, contractName);

            TimesFrameState = TimeFrames.M15;
        }

        private void M30_Click(object sender, RoutedEventArgs e)
        {
            if (TimesFrameState == TimeFrames.M30)
                return;

            var busData = _candleBus.BuildFromM1(timeFrameData, TimeFrame.M30);
            Chart.UpdateData(busData, contractName);

            TimesFrameState = TimeFrames.M30;
        }

        private void H1_Click(object sender, RoutedEventArgs e)
        {
            if (TimesFrameState == TimeFrames.H1)
                return;

            var busData = _candleBus.BuildFromM1(timeFrameData, TimeFrame.H1);
            Chart.UpdateData(busData, contractName);

            TimesFrameState = TimeFrames.H1;
        }

        private void H4_Click(object sender, RoutedEventArgs e)
        {
            if (TimesFrameState == TimeFrames.H4)
                return;

            var busData = _candleBus.BuildFromM1(timeFrameData, TimeFrame.H4);
            Chart.UpdateData(busData, contractName);

            TimesFrameState = TimeFrames.H4;
        }

        private void ETH_Click(object sender, RoutedEventArgs e)
        {
            if (SessionTypeState == SessionType.ETH)
                return;

            timeFrameData = m1Data
                .Where(c => IsETH(c.Hd.Timestamp))
                .ToList();

            var busData = _candleBus.BuildFromM1(timeFrameData, (TimeFrame)TimesFrameState);
            Chart.UpdateData(busData, contractName);

            SessionTypeState = SessionType.ETH;
        }


        private void RTH_Click(object sender, RoutedEventArgs e)
        {
            if (SessionTypeState == SessionType.RTH)
                return;

            timeFrameData = m1Data
                .Where(c => IsRTH(c.Hd.Timestamp))
                .ToList();

            var busData = _candleBus.BuildFromM1(timeFrameData, (TimeFrame)TimesFrameState);
            Chart.UpdateData(busData, contractName);

            SessionTypeState = SessionType.RTH;
        }

    }
}