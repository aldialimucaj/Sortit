using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sortit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Clicking the start button starts the listing and sorting process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            String filePath = txtSourceFolder.Text;
            if (!filePath.EndsWith("\\"))
            {
                filePath += "\\";
            }
            var files = IOUtils.GetAllFiles(filePath, "*.*", _ => _.Exists).ToList();
            foreach(String file in files) 
            {
                tvFilesTree.Items.Add(file);
            }
        }

        private void btnSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            // Show open file dialog box
            DialogResult result = dlg.ShowDialog();
                        
            txtSourceFolder.Text = dlg.SelectedPath; ;
        }

        private void btnDestinationFolder_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
