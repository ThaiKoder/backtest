using System;
using System.Collections.Generic;
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
        public class ChildClickData
        {
            public int Index { get; set; }
        }

        public PanelWindow()
        {
            InitializeComponent();
        }


        private void Killzone_Click(object sender, RoutedEventArgs e)
        {
            var data = new ChildClickData
            {
                Index = 3
            };

            //Clicked?.Invoke(this, data);

        }
    }
}
