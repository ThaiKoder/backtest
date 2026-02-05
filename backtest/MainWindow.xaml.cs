using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
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
using static backtest.PanelWindow;
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
        public KillZones killZones;




        public static IEnumerable<OHLCV> ReadCandlesStream(string filePath)
        {
            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                OHLCV? candle = null;

                try
                {
                    candle = JsonSerializer.Deserialize<OHLCV>(line);
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"JSON invalide ignoré : {ex.Message}");
                }

                if (candle != null)
                    yield return candle;
            }
        }
        public MainWindow()
        {
            ///////////////////////////////
            //READ
            ///////////////////////////////
            //string jsonString = File.ReadAllText(filePath);


            ///////////////////////////////
            //Normalizer
            ///////////////////////////////

            // Dictionnaire des contrats par trimestre
            var quarterContracts = new Dictionary<int, string>
            {
                { 1, "NQM" },
                { 2, "NQU" },
                { 3, "NQZ" },
                { 4, "NQH" }
            };

            // Récupère le trimestre d'une date
            int GetQuarter(DateTime date)
            {
                if (date.Month <= 3) return 1;
                if (date.Month <= 6) return 2;
                if (date.Month <= 9) return 3;
                return 4;
            }

            // Boucle de traitement des bougies
            var m1Data = new List<OHLCV>();

            foreach (var candle in ReadCandlesStream(filePath))
            {
                int quarter = GetQuarter(candle.Hd.Timestamp);
                string contractName = quarterContracts[quarter];

                // Filtre sur le contrat correspondant au trimestre
                if (!candle.Symbol.Trim().StartsWith(contractName, StringComparison.OrdinalIgnoreCase))
                    continue;
                //Debug.WriteLine($"{candle.Symbol.Trim().StartsWith(contractName, StringComparison.OrdinalIgnoreCase)} Candle timestamp: {candle.Hd.Timestamp}, Quarter: Q{quarter}, Expected Contract Prefix: {contractName}, Candle Symbol: {candle.Symbol}");

                var normalized = OHLCVNormalizer.Normalize(candle);
                m1Data.Add(normalized);
            }

            //

            timeFrameData = m1Data;


            

            ///////////////////////////////
            //Candle bus
            ///////////////////////////////



            ///////////////////////////////
            //Chart
            ///////////////////////////////
            Chart = new Chart(m1Data, contractName);
            DataContext = this;

            //KillZones
            killZones = new KillZones(Chart.Model, timeFrameData);

        }


        private PanelWindow _panel; // champ pour garder la fenêtre enfant

        public void OpenPanel()
        {
            // Si le panel est déjà ouvert, on le ramène au premier plan
            if (_panel != null)
            {
                _panel.Activate();
                return;
            }

            // Crée une nouvelle instance du panel, en passant "this" pour référence parent
            _panel = new PanelWindow(this)
            {
                Owner = this // lien minimal pour propriété Owner
            };

            // Quand le panel est fermé, on libère la référence
            _panel.Closed += (s, e) => _panel = null;

            // Affiche le panel
            _panel.Show();
        }



        private void OpenPanelWindow_Click(object sender, RoutedEventArgs e)
        {
            PanelWindow panel = new PanelWindow(this); // on passe "this" au constructeur
            panel.Show();
        }



        // Méthodes publique pour que l'enfant puisse l'appeler

        public void ZoomCandle(DateTime targetTimestamp)
        {
            Chart.ApplyZoomCandle(targetTimestamp);
        }

        public void BacktestAction()
        {
            killZones.Show();
            DateTime targetTimestamp = new DateTime(2026, 1, 18, 2, 0, 0);
            ZoomCandle(targetTimestamp);


        }


        public List<KillZone> getKillZones()
        {
            return killZones.Zones;
        }


        public void ConceptTestKillZone()
        {
            List<KillZone> zones = getKillZones();

            if (zones == null || !zones.Any())
            {
                Debug.WriteLine("Aucune KillZone disponible.");
                return;
            }

            int totalValides = 0;
            int totalInvalides = 0;

            // Grouper par jour
            var zonesParJour = zones.GroupBy(z => z.Start.Date);

            foreach (var jourGroup in zonesParJour)
            {
                var day = jourGroup.Key;
                var london = jourGroup.FirstOrDefault(z => z.Name == "London");
                var nyam = jourGroup.FirstOrDefault(z => z.Name == "NY AM");

                if (london != null && nyam != null)
                {
                    bool condition = london.High > nyam.Low;

                    if (condition)
                        totalValides++;
                    else
                        totalInvalides++;

                    Debug.WriteLine($"{day:yyyy-MM-dd} : London Low={london.Low}, NY AM Low={nyam.Low} => {(condition ? "Valide" : "Invalide")}");
                }
                else
                {
                    Debug.WriteLine($"{day:yyyy-MM-dd} : Zones manquantes pour London ou NY AM");
                }
            }

            Debug.WriteLine($"Total valides : {totalValides}, Total invalides : {totalInvalides}");
        }



        private void backtest_Click(object sender, RoutedEventArgs e)
        {
            OpenPanel();
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
                .Where(c => Chart.IsETH(c.Hd.Timestamp))
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
                .Where(c => Chart.IsRTH(c.Hd.Timestamp))
                .ToList();

            var busData = _candleBus.BuildFromM1(timeFrameData, (TimeFrame)TimesFrameState);
            Chart.UpdateData(busData, contractName);

            SessionTypeState = SessionType.RTH;
        }

        class DominantContractPoint
        {
            public DateTime Timestamp { get; set; }
            public string Symbol { get; set; } = string.Empty;
            public double Volume { get; set; }
        }

        private void devbta_Click(object sender, RoutedEventArgs e)
        {
            List<DominantContractPoint> result = new();

            DateTime? currentTimestamp = null;
            OHLCV? currentDominant = null;
            string? lastSymbol = null;

            // --- Étape 1 : sélection du contrat dominant par timestamp ---
            foreach (var candle in timeFrameData)
            {
                var ts = candle.Hd.Timestamp;

                if (currentTimestamp == null || ts != currentTimestamp)
                {
                    if (currentDominant != null && currentDominant.Symbol != lastSymbol)
                    {
                        result.Add(new DominantContractPoint
                        {
                            Timestamp = currentDominant.Hd.Timestamp,
                            Symbol = currentDominant.Symbol,
                            Volume = currentDominant.Volume
                        });
                        lastSymbol = currentDominant.Symbol;
                    }

                    currentTimestamp = ts;
                    currentDominant = candle;
                }
                else
                {
                    // Même timestamp → garder le plus gros volume
                    if (candle.Volume > currentDominant!.Volume)
                    {
                        currentDominant = candle;
                    }
                }
            }

            // Ajouter le dernier point
            if (currentDominant != null && currentDominant.Symbol != lastSymbol)
            {
                result.Add(new DominantContractPoint
                {
                    Timestamp = currentDominant.Hd.Timestamp,
                    Symbol = currentDominant.Symbol,
                    Volume = currentDominant.Volume
                });
            }

            // --- Affichage classique ---
            //Debug.WriteLine("Timestamp               | Contract | Volume");
            //Debug.WriteLine(new string('-', 55));
            //foreach (var point in result)
            //{
            //    Debug.WriteLine($"{point.Timestamp:yyyy-MM-dd HH:mm:ss} | {point.Symbol,-8} | {point.Volume,10}");
            //}

            // --- Définir le départ de chaque trimestre (2 semaines avant certains mois) ---

            DateTime GetQuarterStart(int year, int quarter)
            {
                return quarter switch
                {
                    1 => new DateTime(year, 3, 1).AddDays(-7),
                    2 => new DateTime(year, 6, 1).AddDays(-7),
                    3 => new DateTime(year, 9, 1).AddDays(-7),
                    4 => new DateTime(year, 12, 1).AddDays(-7),
                    _ => throw new ArgumentException("Quarter must be 1-4")
                };
            }

            int GetQuarter(DateTime dt)
            {
                int year = dt.Year;

                for (int q = 4; q >= 1; q--) // on vérifie du dernier trimestre vers le premier
                {
                    if (dt >= GetQuarterStart(year, q))
                        return q;
                }
                return 1;
            }

            // --- Grouper par trimestre et année ---
            var quarterlyGroups = result
                .GroupBy(d => new
                {
                    Year = d.Timestamp.Year,
                    Quarter = GetQuarter(d.Timestamp)
                });

            // --- Affichage des 2 plus gros contrats par trimestre ---
            Debug.WriteLine("\n--- Top 2 contrats par trimestre ---");
            foreach (var g in quarterlyGroups.OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Quarter))
            {
                var topContracts = g
                    .GroupBy(x => x.Symbol)
                    .Select(x => new { Symbol = x.Key, TotalVolume = x.Sum(v => v.Volume) })
                    .OrderByDescending(x => x.TotalVolume)
                    .Take(2)
                    .ToList();

                Debug.WriteLine($"Year {g.Key.Year}, Q{g.Key.Quarter}:");
                foreach (var c in topContracts)
                {
                    Debug.WriteLine($"  {c.Symbol,-10} | Total Volume: {c.TotalVolume}");
                }
            }
        }
    }
}