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

            int count = 1;
            foreach (var ts in timestamps)
            {
                var texte = $"{ts:yyyy-MM-dd HH:mm}";
                Debug.WriteLine(texte);
                ListeKillZones.Items.Add(texte);
                count++;
            }

        }


        private void ListeKillZones_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListeKillZones.SelectedItem != null)
            {
                string texte = ListeKillZones.SelectedItem.ToString();
                Debug.WriteLine($"Double Clic sur : {texte}");
            }
        }
    }
}
