using Sortit.Properties;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sortit.al.aldi.sortit.views
{
    /// <summary>
    /// Interaction logic for AlphaGrid.xaml
    /// </summary>
    public partial class AlphaGrid : UserControl
    {
        public AlphaGrid()
        {
            InitializeComponent();
        }

        private void saveControlChanges(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox box = sender as TextBox;
                Settings.Default[box.Name] = box.Text;
            }
            else if (sender is CheckBox)
            {
                CheckBox box = sender as CheckBox;
                Settings.Default[box.Name] = !box.IsChecked;
            }

            Properties.Settings.Default.Save();
        }

    }
}
