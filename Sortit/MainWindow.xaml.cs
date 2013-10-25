using Sortit.al.aldi.sortit.control;
using Sortit.al.aldi.sortit.model;
using Sortit.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        // binding console for debug purposes
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public MainWindow()
        {
            InitializeComponent();
            // Console for debug purposes
#if DEBUG
            AllocConsole();
#endif
        }

        /// <summary>
        /// Clicking the start button starts the listing and sorting process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            String filePath = txtSourceFolder.Text.EndsWith("\\") ? txtSourceFolder.Text : txtSourceFolder.Text + "\\";
            IEnumerable<File2Sort> files = null;

            // If alpha numerical files are to be ignored then a different delegate: TODO Might be more elegant
            if (chckIgnoreNonAlpha.IsChecked.Value)
            {
                files = IOUtils.GetAllFiles(filePath, txtPattern.Text, _ => _.IsAlphaNumeric).ToList();
            }
            else
            {
                files = IOUtils.GetAllFiles(filePath, txtPattern.Text).ToList();
            }

            // Adding entries in the treeView
            tvFilesTree.Items.Clear();
            foreach (File2Sort file in files)
            {
                tvFilesTree.Items.Add(file);
            }

            switch (SortingType.Text)
            {
                case "Alphabetically":
                    SortFilesAlpha sfa = new SortFilesAlpha(txtDestinationFolder.Text, Int32.Parse(txtDepth.Text));
                    sfa.sort(files.ToList());
                    if (chckCleanEmptyDir.IsChecked.Value)
                    {
                        IOUtils.CleanEmptyDirs(txtDestinationFolder.Text);
                    }
                    break;
            }

        }

        private void btnSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            // Show open file dialog box
            DialogResult result = dlg.ShowDialog();

            txtSourceFolder.Text = dlg.SelectedPath;

            Settings.Default["dir_source"] = txtSourceFolder.Text;
            Settings.Default.Save();
        }

        private void btnDestinationFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            // Show open file dialog box
            DialogResult result = dlg.ShowDialog();

            txtDestinationFolder.Text = dlg.SelectedPath;

            Settings.Default["dir_destination"] = txtDestinationFolder.Text;
            Settings.Default.Save();
        }


        private void OnExit(object sender, ExitEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void txtDestinationFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default["dir_destination"] = txtDestinationFolder.Text;
            Properties.Settings.Default.Save();
        }

        private void txtSourceFolder_Textchanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default["dir_source"] = txtSourceFolder.Text;
            Properties.Settings.Default.Save();
        }

        private void txtDepth_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default["value_depth"] = Int32.Parse(txtDepth.Text);
            Properties.Settings.Default.Save();
        }

        private void SortingType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default["sort_type"] = SortingType.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void chckCleanEmptyDir_UnCheck(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_clean_dir"] = chckCleanEmptyDir.IsEnabled;
            Properties.Settings.Default.Save();
        }

        private void txtPattern_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default["crawl_pattern"] = txtPattern.Text;
            Settings.Default.Save();
        }

        private void chckIgnoreNonAlpha_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_ignore_non_alpha"] = true;
            Properties.Settings.Default.Save();
        }

        private void chckIgnoreNonAlpha_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_ignore_non_alpha"] = false;
            Properties.Settings.Default.Save();
        }

        private void chckCleanEmptyDir_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_clean_dir"] = false;
            Properties.Settings.Default.Save();
        }

        private void chckCleanEmptyDir_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_clean_dir"] = true;
            Properties.Settings.Default.Save();
        }
    }
}
