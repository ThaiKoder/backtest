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
            //string nouveauTexte = "KillZone " + (ListeKillZones.Items.Count + 1);
            //ListeKillZones.Items.Add(nouveauTexte);
            if (ParentWindow != null)
            {
                // Appeler la méthode du parent
                ParentWindow.BacktestAction();
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
