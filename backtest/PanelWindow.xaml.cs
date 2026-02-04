using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace backtest
{
    /// <summary>
    /// Logique d'interaction pour PanelWindow.xaml
    /// </summary>
    public partial class PanelWindow : Window
    {

        public MainWindow ParentWindow { get; set; }

        public class ChildClickData
        {
            public int Index { get; set; }
        }


        public PanelWindow(MainWindow parent)
        {
            InitializeComponent();
            ParentWindow = parent;
        }


        // Classe pour stocker l'élément avec sa date
        public class KillZoneItem
        {
            public DateTime Timestamp { get; set; }
            public string Texte => Timestamp.ToString("yyyy-MM-dd HH:mm");
        }

        // Dans ton bouton
        private void Killzone_Click(object sender, RoutedEventArgs e)
        {
            if (ParentWindow != null)
            {
                ParentWindow.BacktestAction();
            }

            List<DateTime> timestamps = ParentWindow.getKillZones();

            if (timestamps == null || !timestamps.Any())
            {
                Debug.WriteLine("Aucune KillZone disponible.");
                return;
            }

            ListeKillZones.Items.Clear(); // Pour éviter de dupliquer

            foreach (var ts in timestamps)
            {
                var item = new KillZoneItem { Timestamp = ts };
                ListeKillZones.Items.Add(item);
            }

            // Pour afficher le texte dans la ListBox
            ListeKillZones.DisplayMemberPath = "Texte";
        }



        private void ListeKillZones_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListeKillZones.SelectedItem is KillZoneItem selectedItem)
            {
                DateTime timeStamp = selectedItem.Timestamp;
                Debug.WriteLine($"Date sélectionnée : {timeStamp}");
                ParentWindow.ZoomCandle(timeStamp);
            }
        }
    }
}
