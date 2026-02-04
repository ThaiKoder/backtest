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


        // Classe pour stocker l'élément de la ListBox avec la zone complète
        public class KillZoneItem
        {
            public KillZone Zone { get; set; }

            // Texte à afficher dans la ListBox
            public string Texte => $"{Zone.Start:yyyy-MM-dd HH:mm}";
        }

        // Lors du clic sur le bouton
        private void Killzone_Click(object sender, RoutedEventArgs e)
        {
            if (ParentWindow != null)
            {
                ParentWindow.BacktestAction();
            }

            // Récupérer toutes les zones complètes
            List<KillZone> zones = ParentWindow.getKillZones();

            if (zones == null || !zones.Any())
            {
                Debug.WriteLine("Aucune KillZone disponible.");
                return;
            }

            ListeKillZones.Items.Clear(); // éviter les doublons

            foreach (var zone in zones)
            {
                var item = new KillZoneItem { Zone = zone };
                ListeKillZones.Items.Add(item);
            }

            // Afficher le texte dans la ListBox
            ListeKillZones.DisplayMemberPath = "Texte";
        }

        // Double-clic sur un élément de la ListBox
        private void ListeKillZones_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListeKillZones.SelectedItem is KillZoneItem selectedItem)
            {
                KillZone zone = selectedItem.Zone;
                Debug.WriteLine($"Zone sélectionnée : Start={zone.Start}, End={zone.End}, High={zone.High}, Low={zone.Low}");

                // Exemple : zoom sur la première bougie de la zone
                ParentWindow.ZoomCandle(zone.Start);
            }
        }

    }
}
